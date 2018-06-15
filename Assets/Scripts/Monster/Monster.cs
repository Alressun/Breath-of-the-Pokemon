using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MonsterAI))]
public class Monster : SerializedMonoBehaviour
{
    [OdinSerialize]
    public MonsterData MonsterData { get; set; }

    [OdinSerialize]
    public int PlayerLevel => Battle.Instance.PlayerMonsters.Contains(this)
        ? Battle.Instance.Player.Level
        : Battle.Instance.Enemy.Level;

    [OdinSerialize, ReadOnly]
    private int? _health;
    public int Health
    {
        get
        {
            if (_health == null)
                _health = MaxHealth;
            return _health.GetValueOrDefault();
        }
        set
        {
            _health = value;

            if (_health < 0)
                _health = 0;
        }
    }
    
    public int MaxHealth => (MonsterData.HealthScaling * PlayerLevel / 10) + PlayerLevel + 10;
    public int Attack => (MonsterData.AttackScaling * PlayerLevel / 10) + PlayerLevel + 5;
    public int Defense => (MonsterData.DefenseScaling * PlayerLevel / 10) + PlayerLevel + 5;
    public int Speed => (MonsterData.SpeedScaling * PlayerLevel / 10) + PlayerLevel + 5;

    private List<BattleExecutable> _moves;
    [OdinSerialize, ReadOnly]
    public List<BattleExecutable> Moves
    {
        get
        {
            return _moves ?? (_moves = MonsterData.MoveList.Where(move => move.Item1 <= PlayerLevel)
                       .Select(move => move.Item2).ToList());
        }
        private set
        {
            _moves = value;
        }
    }

    public event EventHandler OnActive;
    public event EventHandler<OnDamageEventArgs> OnDamage;
    public event EventHandler<OnHealEventArgs> OnHeal;
    public event EventHandler<OnDeathEventArgs> OnDeath;

    private MonsterAI _monsterAI;

    private void Awake()
    {
        _monsterAI = GetComponent<MonsterAI>();
    }

    public BattleExecutable SelectMove()
    {
        return _monsterAI.SelectMove();
    }

    public Monster SelectTarget()
    {
        return _monsterAI.SelectTarget();
    }

    public void DealDamage(Monster target, int power)
    {
        target.TakeDamage((float)Attack * power / 25, this);
    }

    public void TakeDamage(float proposedDamage, Monster source)
    {
        var actualDamage = (int)(proposedDamage / Defense);
        Health -= actualDamage;
        OnDamage?.Invoke(this, new OnDamageEventArgs() { DamageAmount = actualDamage, RemainingHealth = Health, DamageSource = source });

        if (Health > 0) return;

        OnDeath?.Invoke(this, new OnDeathEventArgs() { DamageSource = source });
        if (Health > 0) return; //OnDeath may restore health

        gameObject.SetActive(false);
        Battle.Instance.CalculateTurnOrder();
    }

    public void DealHealing(Monster target, int power)
    {
        target.TakeHealing((float)Attack * power / 25, this);
    }

    public void TakeHealing(float proposedAmount, Monster source)
    {
        var actualHealing = (int)proposedAmount;
        Health += actualHealing;
        OnHeal?.Invoke(this, new OnHealEventArgs() { HealAmount = actualHealing, RemainingHealth = Health, DamageSource = source });
    }

#region EventArgs
    public class OnDamageEventArgs : EventArgs
    {
        public int DamageAmount { get; set; }
        public int RemainingHealth { get; set; } 
        public Monster DamageSource { get; set; }
    }

    public class OnHealEventArgs : EventArgs
    {
        public int HealAmount { get; set; }
        public int RemainingHealth { get; set; }
        public Monster DamageSource { get; set; }
    }

    public class OnDeathEventArgs : EventArgs
    {
        public Monster DamageSource { get; set; }
    }
#endregion
}