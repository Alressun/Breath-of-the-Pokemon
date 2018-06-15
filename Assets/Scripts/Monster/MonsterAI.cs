using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Monster))]
public class MonsterAI : SerializedMonoBehaviour
{
    private Monster _monster;

    private void Awake()
    {
        _monster = GetComponent<Monster>();
    }

    public BattleExecutable SelectMove()
    {
        return _monster.Moves.FirstOrDefault();
    }

    public Monster SelectTarget()
    {
        return Battle.Instance.NextMove.ValidTargets.FirstOrDefault();
    }
}