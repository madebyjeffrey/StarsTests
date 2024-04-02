namespace StarsTests1;

class Program
{
    static void Main(string[] args)
    {
        RaceDefinition Human = Predefined.Human2;

        PointCalculator pc = new(Human);

        long points = pc.HabitabilityPoints();

        Console.WriteLine($"Points: {points}");
    }
}
