namespace RaidCalculator.Effects;

public static class EffectSteps
{
    public static void FilterToAllies(Pipeline pipeline)
    {
        pipeline.Champions = pipeline.Champions.Where(c => c.IsChampion).ToArray();
    }

    public static void FilterToLowestHealthChampion(Pipeline pipeline)
    {
        var champion = pipeline.Champions.MinBy(c => c.Health);
        pipeline.Champions = champion is null ? [] : [champion];
    }

    public static void FilterOutCaster(Pipeline pipeline)
    {
        pipeline.Champions = pipeline.Champions.Where(c => c != pipeline.Caster).ToArray();
    }

    public static void FilterToEnemies(Pipeline pipeline)
    {
        pipeline.Champions = pipeline.Champions.Where(c => !c.IsChampion).ToArray();
    }

    public static void FilterToCaster(Pipeline pipeline)
    {
        pipeline.Champions = pipeline.Caster is null ? [] : [pipeline.Caster];
    }

    public static void AddTurnToChampions(Pipeline pipeline)
    {
        foreach (var champion in pipeline.Champions)
            champion.ExtraTurns++;
    }

    public static void IncreaseTurnMeterForChampions(Pipeline pipeline, double value)
    {
        foreach (var champion in pipeline.Champions)
            champion.TurnMeter += value;
    }

    public static void HealChampions(Pipeline pipeline, double value)
    {
        foreach (var champion in pipeline.Champions)
        {
            champion.Health = Math.Min(champion.MaxHp, champion.Health + champion.MaxHp * value);
        }
    }

    public static void ReduceBuffDuration(Pipeline pipeline)
    {
        foreach (var champion in pipeline.Champions)
        {
            foreach (var buff in champion.ActiveBuffs)
            {
                if (buff.TurnsRemaining >= 1)
                    buff.TurnsRemaining--;
            }
        }
    }
}
