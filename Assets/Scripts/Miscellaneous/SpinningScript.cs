using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif

public class SpinningScript : MonoBehaviour
{
    private void Update() => transform.Rotate(axis, SpinningSpeed * Time.deltaTime);

#if UNITY_EDITOR
    private void OnGUI() => Update();
#endif

    [Header("Spinning Settings")]
    [SerializeField] private float SpinningSpeed;
    [SerializeField] private Vector3 axis = Vector3.up;
}