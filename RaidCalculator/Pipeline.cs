using RaidCalculator.Models;

namespace RaidCalculator;

public class Pipeline
{
    public string ActionName { get; set; }
    public ActionContext Context { get; set; } = null!;
    public Champion[] Champions { get; set; } = [];
    public Champion? Caster { get; set; }

    private readonly List<Action<Pipeline>> _steps = [];

    public Pipeline Then(Action<Pipeline> step)
    {
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Runs a step ONLY IF a whole-pipeline condition holds; otherwise skips it
    /// and CARRIES ON with the rest of the pipeline. This is a GATE on ONE fact
    /// about the caster or board — a single bool for the whole set, NOT a
    /// per-champion decision.
    ///
    /// Use when the condition is the same for the whole clause:
    ///   "places [Decrease ATK] on all enemies IF Iron Brago is under [Increase DEF]"
    ///   -> .ThenIf(p => CasterHasBuff(p, "Increase DEF"), applyDecreaseAtk)
    ///
    /// Do NOT use for "each enemy that has a buff" — that's per-champion, use a
    /// FilterWhere step or SplitBy. If your condition reads a single champion's
    /// state, this is the wrong tool.
    /// </summary>
    public Pipeline ThenIf(Func<Pipeline, bool> condition, Action<Pipeline> step)
    {
        _steps.Add(pipeline =>
        {
            if (condition(pipeline))
                step(pipeline);
        });
        return this;
    }
    
    /// <summary>
    /// EITHER/OR: runs exactly ONE of two branches, chosen by a whole-pipeline
    /// condition. The unchosen branch does not run at all. Both branches see the
    /// SAME champion set (the condition picks the path, not who's in it).
    ///
    /// Use for a clause whose SHAPE changes on one fact, where the alternatives
    /// are mutually exclusive:
    ///   "do X to enemies if any are below 4000 HP, OTHERWISE do Y to all allies"
    ///   -> .Branch(p => p.Champions.Any(lowEnemy), doX, doY)
    ///
    /// Contrast with ThenIf (one optional path, no else) and SplitBy (per-champion
    /// routing where BOTH arms run on their own subset). If you find yourself
    /// wanting "some get X, the rest get Y" based on each champion's own state,
    /// that's SplitBy, not Branch — Branch commits everyone to one arm.
    public Pipeline Branch(
        Func<Pipeline, bool> condition,
        Action<Pipeline> ifTrue,
        Action<Pipeline> ifFalse)
    {
        _steps.Add(parent =>
        {
            var child = new Pipeline
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
    
    // <summary>
    /// PARTITION: splits the champion set into two DISJOINT groups by a
    /// PER-CHAMPION predicate — those matching go to 'matched', the rest to
    /// 'rest' — and runs each arm on its own group. Every champion lands in
    /// exactly one arm; the complement is derived automatically (!predicate),
    /// so the two halves can never drift out of sync.
    ///
    /// Use when each champion's OWN state decides its treatment:
    ///   "[Stun] each enemy 2 turns if it has 2+ buffs, else 1 turn"
    ///   -> .SplitBy(c => c.ActiveBuffs.Count(b => b.IsBuff) >= 2,
    ///               matched: m => m.Then(Stun(2)),
    ///               rest:    r => r.Then(Stun(1)))
    ///
    /// Works for single-target for free: a one-champion set just lands entirely
    /// in one arm (the other runs on an empty set, harmless). So the single- and
    /// multi-target versions of the same clause share this exact call.
    ///
    /// NOT the same as Split: Split gives BOTH arms the whole set (overlap);
    /// SplitBy gives each arm HALF (disjoint). Grabbing the wrong one is a silent
    /// bug — with Split here, every champion would get BOTH stun durations.
    /// </summary>
    public Pipeline SplitBy(
        Func<Champion, bool> predicate,
        Action<Pipeline> matched,
        Action<Pipeline> rest)
    {
        _steps.Add(parent =>
        {
            var matchedChampions = parent.Champions.Where(predicate).ToArray();
            var restChampions = parent.Champions.Where(c => !predicate(c)).ToArray();

            var left = new Pipeline
            {
                Context = parent.Context,
                Champions = matchedChampions,
                Caster = parent.Caster
            };
            matched(left);
            left.Run();

            var right = new Pipeline
            {
                Context = parent.Context,
                Champions = restChampions,
                Caster = parent.Caster
            };
            rest(right);
            right.Run();
        });

        return this;
    }

    // <summary>
    /// FORK: runs TWO independent sub-sequences, each on the FULL champion set
    /// (a shared copy — the arms OVERLAP, they are not a partition). Both always
    /// run. Use when one clause does two unrelated things to overlapping targets:
    ///   "do three things to poisoned enemies, AND damage all enemies"
    ///   -> left filters to poisoned and applies; right damages everyone.
    ///
    /// NOT the same as SplitBy: here both arms see EVERY champion, so if both
    /// arms apply to the same champion, they both hit it. Reach for SplitBy when
    /// the two treatments are mutually exclusive per champion; reach for Split
    /// when they're independent and may overlap.
    ///
    /// CAUTION: 'left' runs fully before 'right', and both share the same champion
    /// OBJECTS. If left mutates a champion (applies a debuff), right sees that
    /// mutation. SplitBy doesn't have this hazard because its arms are disjoint.
    /// </summary>
    public Pipeline Split(
        Action<Pipeline> configureLeft,
        Action<Pipeline> configureRight)
    {
        _steps.Add(parent =>
        {
            var left = new Pipeline
            {
                Context = parent.Context,
                Champions = parent.Champions,
                Caster = parent.Caster
            };
            configureLeft(left);
            left.Run();

            var right = new Pipeline
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