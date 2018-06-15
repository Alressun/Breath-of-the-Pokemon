using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BattleDisplay))]
public class Battle : Singleton<Battle>
{
    public int Turn { get; private set; }
    public Trainer Player { get; private set; }
    public Trainer Enemy { get; private set; }
    public BattleExecutable NextMove { get; set; }

    private List<Monster> _playerMonsters = new List<Monster>();
    public List<Monster> PlayerMonsters
    {
        get
        {
            _playerMonsters.RemoveAll(monster => monster.gameObject.activeSelf == false);
            return _playerMonsters;
        }
        private set { _playerMonsters = value; }

    }

    private List<Monster>_enemyMonsters = new List<Monster>();
    public List<Monster> EnemyMonsters
    {
        get
        {
            _enemyMonsters.RemoveAll(monster => monster.gameObject.activeSelf == false);
            return _enemyMonsters;
        }
        private set { _enemyMonsters = value; }

    }

    [OdinSerialize]
    private GameObject _fieldRoot, _battleRoot;
    [OdinSerialize]
    private Transform[] _monsterPlacement = new Transform[4];


    public async void StartBattle(Trainer player, Trainer enemy)
    {
        Player = player;
        Enemy = enemy;
        Turn = 1;

        InitializeBattle(player, enemy);

        do
        {
            GetNextTurn();

            Monster target = null;
            do
            {
                BattleInput.Instance.CurrentInputMode = BattleInput.InputMode.Action;
                NextMove = await CurrentMonsterTurn.Item1.SelectMove(CurrentMonsterTurn.Item2);

                BattleInput.Instance.CurrentInputMode = BattleInput.InputMode.Target;
                if (NextMove.ValidTargets.Count == 0) break;
                target = await CurrentMonsterTurn.Item1.SelectTarget(CurrentMonsterTurn.Item2);
            } while (target == null);

            BattleInput.Instance.CurrentInputMode = BattleInput.InputMode.NoInput;
            await NextMove.Execute(CurrentMonsterTurn.Item2, target);
            NextMove = null;

            Turn++;
        } while (!Player.Team.IsDefeated && !Enemy.Team.IsDefeated);

        CleanupBattle();
    }

    #region Initialization/Cleanup
    public void InitializeBattle(Trainer player, Trainer enemy)
    {
        _battleRoot?.SetActive(true);
        _fieldRoot?.SetActive(false);

        for (var i = 0; i < player.Team.ActiveMonsters.Count; i++)
        {
            player.Team.ActiveMonsters[i].transform.SetParent(_monsterPlacement[i].transform, false);
            player.Team.ActiveMonsters[i].gameObject.SetActive(true);
            PlayerMonsters.Add(player.Team.ActiveMonsters[i]);
        }

        for (var i = 0; i < enemy.Team.ActiveMonsters.Count; i++)
        {
            enemy.Team.ActiveMonsters[i].transform.SetParent(_monsterPlacement[i + 2].transform, false);
            enemy.Team.ActiveMonsters[i].gameObject.SetActive(true);
            EnemyMonsters.Add(enemy.Team.ActiveMonsters[i]);
        }

        CalculateTurnOrder();
        BattleDisplay.Instance.Initialize();
    }

    public void CleanupBattle()
    {
        BattleDisplay.Instance.Cleanup();

        foreach (var monster in PlayerMonsters)
        {
            monster.gameObject.SetActive(false);
        }
        foreach (var monster in EnemyMonsters)
        {
            monster.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
        _fieldRoot?.SetActive(true);
    }
    #endregion

    #region Turn selection
    public enum Sides { Ally, Enemy }
    public Tuple<Trainer, Monster> CurrentMonsterTurn { get; private set; }
    public List<Tuple<Monster, int>> TurnOrder { get; set; } //int is for a random number that breaks speed ties

    public void CalculateTurnOrder()
    {
        if (TurnOrder == null)
        {
            var activeMonsters = PlayerMonsters.Union(EnemyMonsters).ToList();
            var randomNumbers = new List<int>();

            foreach (var unused in activeMonsters)
            {
                var newRandom = UnityEngine.Random.Range(0, 1000);
                while (randomNumbers.Contains(newRandom))
                {
                    newRandom = UnityEngine.Random.Range(0, 1000);
                }
                randomNumbers.Add(newRandom);
            }

            using (var randomEnumerator = randomNumbers.GetEnumerator())
            {
                TurnOrder = activeMonsters.Select(monster =>
                {
                    randomEnumerator.MoveNext();
                    return new Tuple<Monster, int>(monster, randomEnumerator.Current);
                }).ToList();
            }
        }

        TurnOrder.RemoveAll(monster => !PlayerMonsters.Contains(monster.Item1) && !EnemyMonsters.Contains(monster.Item1));
        TurnOrder = TurnOrder.OrderByDescending(monster => monster.Item1.MonsterData.SpeedScaling)
            .ThenBy(monster => monster.Item2).ToList();
    }

    private void GetNextTurn()
    {
        var nextTurnIndex = CurrentMonsterTurn != null ? TurnOrder.FindIndex(monster => monster.Item1 == CurrentMonsterTurn.Item2) + 1 : 0;
        var nextMonster = TurnOrder[nextTurnIndex == TurnOrder.Count ? 0 : nextTurnIndex].Item1;

        CurrentMonsterTurn = new Tuple<Trainer, Monster>(
            PlayerMonsters.Contains(nextMonster) ? Player : Enemy, nextMonster);
    }
    #endregion
}