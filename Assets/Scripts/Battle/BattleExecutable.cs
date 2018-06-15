using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class BattleExecutable : SerializedScriptableObject
{
    [OdinSerialize]
    public string Name { get; private set; }
    
    [OdinSerialize]
    public bool TargetsAllies { get; private set; }
    [OdinSerialize]
    public bool TargetsEnemies { get; private set; }
    [OdinSerialize]
    public bool HitsAllTargets { get; private set; }

    public List<Monster> ValidTargets
    {
        get
        {
            var monsters = new List<Monster>();
            
            if (TargetsAllies)
                monsters.AddRange(Battle.Instance.CurrentMonsterTurn.Item1 != Battle.Instance.Player ? Battle.Instance.PlayerMonsters : Battle.Instance.EnemyMonsters);
            if (TargetsEnemies)
                monsters.AddRange(Battle.Instance.CurrentMonsterTurn.Item1 == Battle.Instance.Player ? Battle.Instance.EnemyMonsters : Battle.Instance.PlayerMonsters);

            return monsters;
        }
    }

    public abstract Task Execute(Monster source, Monster target);
}