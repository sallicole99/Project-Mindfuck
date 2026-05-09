using UnityEngine;

[ExecuteInEditMode]
public class PickupAnimationScript : MonoBehaviour
{
    #region Rendering & Animation Logic
    private void OnWillRenderObject()
    {
        var cam = Camera.current;
        if (!cam || (updateTimer += Time.deltaTime) < updateInterval) return;

        updateTimer = 0;

        float dist = Vector3.Distance(transform.position, cam.transform.position);
        if (dist > maxDistance) return;

        updateInterval = dist / UpdateFrequency;
        transform.localPosition = Vector3.up * (Mathf.Sin(Time.time * 2) * amplitude + centerY);
    }
    #endregion

    #region Serialized Configuration
    [Header("Animation Settings")]
    [SerializeField] private float amplitude = 0.5f;
    [SerializeField] private float centerY = 1f, maxDistance = 125f;
    #endregion

    #region Internal State
    private const float UpdateFrequency = 400f;
    private float updateTimer;
    private float updateInterval;
    #endregion
}