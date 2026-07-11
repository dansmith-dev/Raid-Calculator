namespace RaidCalculator.Effects;

public class EffectPipeline
{
    public ActionContext Context { get; set; } = null!;
    public Champion[] Champions { get; set; } = [];
    public Champion? Caster { get; set; }

    private readonly List<Action<EffectPipeline>> _steps = [];

    public EffectPipeline Then(Action<EffectPipeline> step)
    {
        _steps.Add(step);
        return this;
    }

    public EffectPipeline ThenIf(Func<EffectPipeline, bool> condition, Action<EffectPipeline> step)
    {
        _steps.Add(pipeline =>
        {
            if (condition(pipeline))
                step(pipeline);
        });
        return this;
    }
    
    public EffectPipeline Branch(
        Func<EffectPipeline, bool> condition,
        Action<EffectPipeline> ifTrue,
        Action<EffectPipeline> ifFalse)
    {
        _steps.Add(parent =>
        {
            var child = new EffectPipeline
            {
                Context = parent.Context,
                Champions = parent.Champions,
                Caster = parent.Caster
            };
            (condition(child) ? ifTrue : ifFalse)(child);
            child.Run();
        });
        return this;
    }

    public EffectPipeline Split(
        Action<EffectPipeline> configureLeft,
        Action<EffectPipeline> configureRight)
    {
        _steps.Add(parent =>
        {
            var left = new EffectPipeline
            {
                Context = parent.Context,
                Champions = parent.Champions,
                Caster = parent.Caster
            };
            configureLeft(left);
            left.Run();

            var right = new EffectPipeline
            {
                Context = parent.Context,
                Champions = parent.Champions,
                Caster = parent.Caster
            };
            configureRight(right);
            right.Run();
        });

        return this;
    }

    public void Run()
    {
        foreach (var step in _steps)
            step(this);
    }
}
