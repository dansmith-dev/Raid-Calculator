namespace RaidCalculator.Buffs;

public class AppliedBuff
{
    public string Name;
    public int TurnsRemaining;
    public double? Multiplier;
    public double? FlatRate;
    public ChampionStat? AffectedStat;
    public BuffType BuffType;

    public AppliedBuff(
        string name,
        int turnsRemaining,
        BuffType buffType,
        ChampionStat? affectedStat,
        double? multiplier,
        double? flatRate)
    {
        Name = name;
        TurnsRemaining = turnsRemaining;
        AffectedStat = affectedStat;
        Multiplier = multiplier;
        FlatRate = flatRate;
        BuffType = buffType;
    }
}
