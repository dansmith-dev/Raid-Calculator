namespace RaidCalculator.Buffs;

public class ChancedStun(string name = "Chanced Stunn", int duration = 1) : Buff(name, duration)
{
    public override void ConfigureBuffPipeline(BuffPipeline buffPipeline)
    {
        buffPipeline
            .ThenIf(
                _ => PipelineConditions.Chance(0.75),
                steps => steps
                    .Then(BuffSteps.FilterToOpponents)
                    .Then(p => BuffSteps.StunChampions(p, duration))
            );
    }
}