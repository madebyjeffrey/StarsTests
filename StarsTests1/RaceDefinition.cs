namespace StarsTests1;

public enum HabType
{
    Gravity,
    Temperature,
    Radiation
};

public class RaceDefinition
{
    public required GameRules GameRules { get; set; }

    public required bool TotalTerraforming { get; init; }

    public required List<EnvironmentCondition> EnvironmentConditions { get; set; }
}
