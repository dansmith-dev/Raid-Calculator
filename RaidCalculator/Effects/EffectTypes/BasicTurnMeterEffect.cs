
namespace RaidCalculator.Effects;

public class BasicTurnMeterEffect : Effect
{
    public BasicTurnMeterEffect()
    {
        Name = "Ally Turn Meter Fill";
    }

    public override void ConfigureEffectPipeline(Pipeline pipeline)
    {
        pipeline
            .Then(EffectSteps.FilterToAllies)
            .Then(p => EffectSteps.IncreaseTurnMeterForChampions(p, 15));
    }
}
