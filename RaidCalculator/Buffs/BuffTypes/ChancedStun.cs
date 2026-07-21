namespace RaidCalculator.Buffs;

public class ChancedStun(string name = "Chanced Stunn", int duration = 1) : Buff(name, duration)
{
    public override void ConfigureBuffPipeline(Pipeline buffPipeline)
    {
        buffPipeline
            .Then(BuffSteps.FilterToOpponents)
            .Then(BuffSteps.FilterByChance(0.75))
            .SplitBy(
                predicate: c => c.ActiveBuffs.Count >= 2,
                matched:   m => m.Then(BuffSteps.Stun(2)),
                rest:      r => r.Then(BuffSteps.Stun(1)));
    }
}