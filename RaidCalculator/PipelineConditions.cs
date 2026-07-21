namespace RaidCalculator;

public static class PipelineConditions
{
    public static bool Chance(double chance)
    {
        var random = new Random();

        return random.NextDouble() > chance;
    }
}