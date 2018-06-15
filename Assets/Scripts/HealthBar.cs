using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Monster Target { get; set; }

    private float _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale.x;
    }

    private void Update()
    {
        transform.localScale = new Vector3(_originalScale * Target.Health / Target.MaxHealth, transform.localScale.y, transform.localScale.z);
        transform.localPosition = new Vector3(-(1 - (float)Target.Health / Target.MaxHealth) / 2, transform.localPosition.y, transform.localPosition.z);
    }
}