using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[RequireComponent(typeof(Trainer))]
public class TrainerBattle : SerializedMonoBehaviour
{
    public Trainer Trainer { get; set; }

    public Monster bobby;

    public bool Battled { get; set; }

    private void Awake()
    {
        Trainer = GetComponent<Trainer>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(Tags.Player) || Battled) return;

        var playerTrainer = collision.gameObject.GetComponent<Trainer>();
        if (playerTrainer != null)
        {
            Battle.Instance.StartBattle(playerTrainer, Trainer);
            Battled = true;
        }

        else Debug.LogError("Trainer not found!");
    }
}