using RaidCalculator.Buffs;

namespace RaidCalculator.Effects;

public class ChancedReduceBuffDuration: Effect
{
    public ChancedReduceBuffDuration()
    {
        Name = "Chanced Reduce Buff Duration";
    }
    
    public override void ConfigureEffectPipeline(EffectPipeline pipeline)
    {
        // Has a 20% chance of decreasing the duration of all buffs by 1 turn.
        pipeline
            .ThenIf(
                _ => PipelineConditions.Chance(0.2),
                p => p
                    .Then(EffectSteps.ReduceBuffDuration));
    }
}