namespace RaidCalculator.Buffs;

public static class BuffSteps
{
    // ── No-arg steps: already Action<Pipeline>-shaped, used as method groups. Unchanged. ──

    public static void FilterToAllies(Pipeline pipeline)
    {
        pipeline.Champions = pipeline.Champions.Where(c => c.IsChampion).ToArray();
    }

    public static void FilterToEnemies(Pipeline pipeline)
    {
        pipeline.Champions = pipeline.Champions.Where(c => !c.IsChampion).ToArray();
    }

    public static void FilterToOpponents(Pipeline pipeline)
    {
        if (pipeline.Caster.IsChampion)
            pipeline.Champions = pipeline.Champions.Where(c => !c.IsChampion).ToArray();
        else
            pipeline.Champions = pipeline.Champions.Where(c => c.IsChampion).ToArray();
    }

    public static void FilterToCaster(Pipeline pipeline)
    {
        pipeline.Champions = pipeline.Caster is null ? [] : [pipeline.Caster];
    }

    // ── Parameterised steps: factories. Call as FilterByChance(0.75), Stun(2), etc. ──

    public static Action<Pipeline> FilterByChance(double chance) =>
        p => p.Champions = p.Champions
            .Where(_ => p.Context.Rng.NextDouble() <= chance)
            .ToArray();

    public static Action<Pipeline> ApplySpeedMultiplier(double value, int duration) =>
        ApplyStatMultiplier(ChampionStat.Speed, value, duration);

    public static Action<Pipeline> ApplyDefenceMultiplier(double value, int duration) =>
        ApplyStatMultiplier(ChampionStat.Defence, value, duration);

    public static Action<Pipeline> Stun(int duration) =>
        p =>
        {
            foreach (var champion in p.Champions)
                champion.ActiveBuffs.Add(
                    new AppliedBuff("Stun", duration, BuffType.Stun, null, null, null));
        };

    public static Action<Pipeline> ApplyStatMultiplier(
        ChampionStat stat,
        double multiplier,
        int duration) =>
        p =>
        {
            foreach (var champion in p.Champions)
            {
                var existing = champion.ActiveBuffs.FirstOrDefault(b =>
                    b.Name == p.ActionName && b.AffectedStat == stat);

                if (existing is null)
                {
                    champion.ActiveBuffs.Add(
                        new AppliedBuff(p.ActionName, duration, BuffType.Stat, stat, multiplier, null));
                    continue;
                }

                existing.TurnsRemaining = Math.Max(existing.TurnsRemaining, duration);
                if (existing.Multiplier is null || multiplier > existing.Multiplier.Value)
                    existing.Multiplier = multiplier;
            }
        };
}