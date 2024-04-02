namespace StarsTests1;

public static class Predefined
{
    private static string FormatGravityValue(int value, bool immune)
    {
        if (immune)
        {
            return "Immune";
        }
        else
        {
            double gravity = Math.Pow(4, (Convert.ToDouble(value) - 50) / 50);

            return $"{gravity:F2}g";
        }
    }

    private static string FormatTemperatureValue(int value, bool immune)
    {
        if (immune)
        {
            return "Immune";
        }
        else
        {
            int temperature = (value - 50) * 4;

            return $"{temperature}ºC";
        }
    }

    private static string FormatRadiationValue(int value, bool immune)
    {
        if (immune)
        {
            return "Immune";
        }
        else
        {
            return $"{value}mR";
        }
    }

    public static GameRules DefaultGame =>
        new GameRules()
        {
            EnvironmentTypes =
            [
                new EnvironmentType(FormatGravityValue),
                new EnvironmentType(FormatTemperatureValue),
                new EnvironmentType(FormatRadiationValue)
            ]
        };

    public static RaceDefinition Human =>
        new RaceDefinition()
        {
            GameRules = DefaultGame,
            TotalTerraforming = false,
            EnvironmentConditions =
            [
                new EnvironmentRange(DefaultGame.EnvironmentTypes[0], 15, 85),
                new EnvironmentRange(DefaultGame.EnvironmentTypes[1], 15, 85),
                new EnvironmentRange(DefaultGame.EnvironmentTypes[2], 15, 85)
            ]
        };

    public static RaceDefinition Human2 =>
        new RaceDefinition()
        {
            GameRules = DefaultGame,
            TotalTerraforming = false,
            EnvironmentConditions =
            [
                new EnvironmentRange(DefaultGame.EnvironmentTypes[0], 15, 85),
                new EnvironmentImmune(DefaultGame.EnvironmentTypes[1]),
                new EnvironmentRange(DefaultGame.EnvironmentTypes[2], 35, 65)
            ]
        };
}
