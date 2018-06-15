using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class Team
{
    public Trainer Trainer { get; set; }

    [OdinSerialize]
    private List<Monster> _monsterPrefabs = new List<Monster>(6);
    [OdinSerialize, HideInInspector]
    public List<Monster> ActiveMonsters { get; set; } = new List<Monster>(2);
    [OdinSerialize, HideInInspector]
    public List<Monster> PassiveMonsters { get; set; } = new List<Monster>(2);
    [OdinSerialize, HideInInspector]
    public List<Monster> BenchedMonsters { get; set; } = new List<Monster>(2);

    public List<Monster> AllMonsters => ActiveMonsters.Concat(PassiveMonsters).Concat(BenchedMonsters).ToList();

    public bool IsDefeated => !AllMonsters.Any(monster => monster.Health > 0);

    public void Init()
    {
        for (var i = 0; i < _monsterPrefabs.Count; i++)
        {
            if (i >= 0 && i < 2)
            {
                var monster = Object.Instantiate(_monsterPrefabs[i]);
                ActiveMonsters.Add(monster);
                monster.gameObject.SetActive(false);
            }
            else if (i >= 2 && i < 4)
            {
                var monster = Object.Instantiate(_monsterPrefabs[i]);
                PassiveMonsters.Add(monster);
                monster.gameObject.SetActive(false);
            }
            else if (i >= 4 && i < 6)
            {
                var monster = Object.Instantiate(_monsterPrefabs[i]);
                BenchedMonsters.Add(monster);
                monster.gameObject.SetActive(false);
            }
        }
    }

    public void Cleanup()
    {
        foreach (var monster in AllMonsters)
        {
            Object.Destroy(monster);
        }
    }
}
