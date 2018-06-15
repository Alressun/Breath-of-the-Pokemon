using Sirenix.Serialization;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Move", menuName = "Damage Move", order = 1)]
public class DamageMove : BattleExecutable
{
    [OdinSerialize]
    public int Power { get; set; }

    public override async Task Execute(Monster source, Monster target)
    {
        source.DealDamage(target, Power);
        await Task.Delay(1000);
    }
}