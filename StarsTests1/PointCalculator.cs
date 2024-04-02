using System.Collections;
using System.Collections.Immutable;

namespace StarsTests1;

using EnvironmentPoint = (int Point, int TerraformOffset);

public class PointCalculator(RaceDefinition race)
{
    public long HabitabilityPoints()
    {
        double points = 0.0;

        foreach (int loopIndex in (int[])[0, 1, 2])
        {
            var environmentPoints = race
                .EnvironmentConditions.Select(condition =>
                    GetPlanetEnvironmentPoints(condition, loopIndex)
                )
                .ToList();

            IEnumerable<IEnumerable<EnvironmentPoint>> emptyProduct = new[]
            {
                Enumerable.Empty<EnvironmentPoint>()
            };

            // This generates the cartesian product of all the habs
            var environmentPointsProduct = environmentPoints.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item })
            );

            List<long> desirability = CalculatePlanetDesirability(
                    environmentPointsProduct,
                    loopIndex
                )
                .ToList();

            // we have to start at the end
            foreach (var condition in Enumerable.Reverse(race.EnvironmentConditions))
            {
                if (condition is EnvironmentRange range)
                {
                    desirability = desirability
                        .Chunk(11)
                        .Select(x => x.Sum() * TestHabWidth(condition, loopIndex) / 100)
                        .ToList();
                }
                else
                {
                    desirability = desirability.Select(x => x * 11).ToList();
                }
            }

            points += desirability.Sum();
        }

        return (long)(points / 10.0 + 0.5);
    }

    private int Iterations(EnvironmentCondition environmentCondition) =>
        environmentCondition switch
        {
            EnvironmentImmune => 1,
            _ => 11
        };

    private int TotalTerraformingCorrectionFactor(int loopIndex) =>
        (loopIndex, race.TotalTerraforming) switch
        {
            (1, true) => 8,
            (1, false) => 5,
            (2, true) => 17,
            (2, false) => 15,
            _ => 0
        };

    private IEnumerable<long> CalculatePlanetDesirability(
        IEnumerable<IEnumerable<EnvironmentPoint>> points,
        int loopIndex
    )
    {
        return points.Select(pt =>
        {
            IList<EnvironmentPoint> pt2 = pt.ToList();

            int[] hab = pt2.Select(x => x.Point).ToArray();
            int terraformOffsetSum = pt2.Select(x => x.TerraformOffset).Sum();

            long planetDesirability = GetEnvironmentPointHabitability(
                race.GameRules.EnvironmentTypes.Zip(
                    hab,
                    (type, hab) => new EnvironmentValue(type, hab)
                )
            );

            if (terraformOffsetSum > TotalTerraformingCorrectionFactor(loopIndex))
            {
                // Console.WriteLine($"B: Modified by offset");
                // bring the planet desirability down by the difference between the terraformOffsetSum and the TTCorrectionFactor
                planetDesirability -=
                    terraformOffsetSum - TotalTerraformingCorrectionFactor(loopIndex);
                // make sure the planet isn't negative in desirability
                if (planetDesirability < 0)
                {
                    planetDesirability = 0;
                }
            }

            planetDesirability *= planetDesirability;

            planetDesirability *= loopIndex switch
            {
                0 => 7,
                1 => 5,
                _ => 6
            };

            return planetDesirability;
        });
    }

    private int TestHabStart(EnvironmentCondition hab, int loopIndex) =>
        hab switch
        {
            EnvironmentImmune => 50,
            EnvironmentRange v
                => Math.Clamp(v.Minimum - TotalTerraformingCorrectionFactor(loopIndex), 0, 100)
        };

    private int TestHabWidth(EnvironmentCondition hab, int loopIndex) =>
        hab switch
        {
            EnvironmentImmune => 11,
            EnvironmentRange v
                => Math.Clamp(v.Maximum + TotalTerraformingCorrectionFactor(loopIndex), 0, 100)
                    - TestHabStart(hab, loopIndex)
        };

    private List<EnvironmentPoint> GetPlanetEnvironmentPoints(
        EnvironmentCondition hab,
        int loopIndex
    )
    {
        int ttCorrectionFactor = TotalTerraformingCorrectionFactor(loopIndex);
        int numIterations = Iterations(hab);

        int testHabStart = TestHabStart(hab, loopIndex);

        int testHabWidth = TestHabWidth(hab, loopIndex);

        return Enumerable
            .Range(0, numIterations)
            .Select(i =>
            {
                int planetHab = numIterations switch
                {
                    <= 1 => testHabStart, // this is because of 0 denominator in next line, only case is immune
                    _ => (testHabWidth * i) / (numIterations - 1) + testHabStart
                };

                // if we on a main loop other than the first one, do some
                // stuff with the terraforming correction factor
                if (loopIndex != 0 && hab is EnvironmentRange ev)
                {
                    int habCentre = ev.Centre();
                    int offset = habCentre - planetHab;

                    int terraformOffset = offset switch
                    {
                        var o when Math.Abs(o) < ttCorrectionFactor => 0,
                        < 0 => offset + ttCorrectionFactor,
                        _ => offset - ttCorrectionFactor
                    };

                    return (habCentre - terraformOffset, terraformOffset);
                }

                return (planetHab, 0);
            })
            .ToList();
    }

    private int GetEnvironmentPointHabitability(IEnumerable<EnvironmentValue> points)
    {
        var environments = race.EnvironmentConditions.Zip(points);

        int planetValuePoints = 0;
        int ideality = 10000;
        int redValue = 0;

        foreach (var (condition, value) in environments)
        {
            (int planetValue, ideality, int red) = GetEnvironmentValueHabitability(
                condition,
                value,
                ideality
            );

            redValue += red;
            planetValuePoints += planetValue;
        }

        if (redValue != 0)
        {
            return -redValue;
        }

        planetValuePoints = (int)(Math.Sqrt((double)planetValuePoints / 3.0) + 0.9);
        planetValuePoints = planetValuePoints * ideality / 10000;

        return planetValuePoints;
    }

    private (int PlanetValue, int Ideality, int RedValue) GetEnvironmentValueHabitability(
        EnvironmentCondition environmentCondition,
        EnvironmentValue environmentValue,
        int ideality
    )
    {
        return (environmentCondition, environmentValue) switch
        {
            (EnvironmentImmune, _) => (10000, ideality, 0),
            (EnvironmentRange range, EnvironmentValue(_, var value))
                when range.Minimum <= value && range.Maximum >= value
                => GetGreenPlanetValue(range, value, ideality),
            (EnvironmentRange range, EnvironmentValue(_, var value))
                => GetRedPlanetValue(range, value, ideality),
            _ => throw new Exception("Should never reach here")
        };
    }

    private (int PlanetValue, int Ideality, int RedValue) GetGreenPlanetValue(
        EnvironmentRange environmentRange,
        int value,
        int ideality
    )
    {
        int habRadius = (environmentRange.Maximum - environmentRange.Minimum) / 2;
        int fromIdeal = 100 - (Math.Abs(value - environmentRange.Centre()) * 100 / habRadius);
        int poorPlanetMod = Math.Abs(value - environmentRange.Centre()) * 2 - habRadius;

        int planetValuePoints = fromIdeal * fromIdeal;

        if (poorPlanetMod > 0)
        {
            ideality *= (habRadius * 2 - poorPlanetMod);
            ideality /= habRadius * 2;
        }

        return (planetValuePoints, ideality, 0);
    }

    private (int PlanetValue, int Ideality, int RedValue) GetRedPlanetValue(
        EnvironmentRange environmentRange,
        int value,
        int ideality
    )
    {
        return (
            0,
            ideality,
            Math.Min(
                (environmentRange.Minimum <= value)
                    ? value - environmentRange.Maximum
                    : environmentRange.Minimum - value,
                15
            )
        );
    }
}
