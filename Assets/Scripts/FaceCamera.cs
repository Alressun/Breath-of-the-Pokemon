using Sirenix.OdinInspector;
using UnityEngine;

public class FaceCamera : SerializedMonoBehaviour
{
    private void Update()
    {
        transform.LookAt(Camera.main.transform);
	}
}