using Sirenix.OdinInspector;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(AICharacterControl))]
public class FollowAI : SerializedMonoBehaviour
{
    private const float FollowDistance = 3f;

    private GameObject _player;
    private AICharacterControl _control;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player");
        _control = GetComponent<AICharacterControl>();

        if (_player == null) Debug.LogError("Player not found");
    }

    private void Update ()
    {
        var playerXZ = new Vector2(_player.transform.position.x, _player.transform.position.z);
        var thisXZ = new Vector2(transform.position.x, transform.position.z);

        _control.SetTarget((playerXZ - thisXZ).magnitude > FollowDistance ? _player.transform : transform);
    }
}
