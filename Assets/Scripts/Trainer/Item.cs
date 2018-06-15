using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item", order = 2)]
public class Item : BattleExecutable
{
    public override async Task Execute(Monster source, Monster target)
    {

    }
}