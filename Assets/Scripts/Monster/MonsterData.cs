using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Monster", menuName = "Monster", order = 0)]
public class MonsterData : SerializedScriptableObject
{
    #region Battle info
    public int HealthScaling;
    public int AttackScaling;
    public int DefenseScaling;
    public int SpeedScaling;
    public List<Tuple<int, BattleExecutable>> MoveList = new List<Tuple<int, BattleExecutable>>();

    public UnityEvent PassiveBattleEffect;
    public UnityEvent BackupEffect;
    #endregion

    #region Field info
    public UnityEvent FieldActiveEffect1;
    public UnityEvent FieldActiveEffect2;
    public UnityEvent FieldPassiveFieldEffect;
    #endregion

    #region Display info
    public Mesh Mesh;
    #endregion
}