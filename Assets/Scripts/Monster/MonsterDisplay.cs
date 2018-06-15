using UnityEngine;

[RequireComponent(typeof(Monster), typeof(MeshFilter))]
public class MonsterDisplay : MonoBehaviour
{
    private Monster _monster;
    private MeshFilter _meshFilter;

    private void Awake()
    {
        _monster = GetComponent<Monster>();

        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.mesh = _monster.MonsterData.Mesh;
    }

    private void Update ()
    {
        
	}
}