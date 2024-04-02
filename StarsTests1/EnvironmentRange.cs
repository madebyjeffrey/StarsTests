namespace StarsTests1;

public record EnvironmentType(Func<int, bool, string> Display);

public interface EnvironmentCondition
{
    abstract EnvironmentType Type { get; }
    abstract int Minimum { get; }
    abstract int Maximum { get; }
    abstract bool IsImmune { get; }
}

public static class EnvironmentConditionExtensions
{
    public static int Centre(this EnvironmentCondition condition) =>
        (condition.Minimum + condition.Maximum) / 2;
}

public record EnvironmentRange(EnvironmentType Type, int Minimum, int Maximum)
    : EnvironmentCondition
{
    public int Centre => (Maximum + Minimum) >> 1;

    public int Span => (Maximum - Minimum);

    public bool IsImmune => false;
}

public record EnvironmentImmune(EnvironmentType Type) : EnvironmentCondition
{
    public bool IsImmune => true;
    public int Minimum => 45;
    public int Maximum => 56;

    int Centre => (Maximum + Minimum) >> 1;
}

public record EnvironmentValue(EnvironmentType Type, int Value);
