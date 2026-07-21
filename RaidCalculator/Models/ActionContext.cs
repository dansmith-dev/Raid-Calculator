using RaidCalculator.Models.Results;
using RaidCalculator.Results;

namespace RaidCalculator.Models;

public class ActionContext
{
    public Champion Caster { get; set; } = null!;
    public Champion[] Targets { get; set; } = [];
    public Skill? Skill { get; set; }

    public List<EffectResult> EffectResults { get; } = [];
    public List<BuffResult> BuffResults { get; } = [];
    public AttackResult? AttackResult { get; set; }
    
    public Random Rng = new Random();
}
