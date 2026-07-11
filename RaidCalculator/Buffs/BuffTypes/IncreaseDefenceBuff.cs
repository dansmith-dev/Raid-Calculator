namespace RaidCalculator.Buffs;

public class IncreaseDefenceBuff(int duration = 3, double defenceMultiplier = 1.6) : Buff("Defence Buff", duration)
{
    public double DefenceMultiplier = defenceMultiplier;

    public override void ConfigureBuffPipeline(BuffPipeline buffPipeline)
    {
        buffPipeline
            .Then(BuffSteps.FilterToAllies)
            .Then(p => BuffSteps.ApplyDefenceMultiplierBuffToChampions(p, DefenceMultiplier, BuffDuration));
    }
}
