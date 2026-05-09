using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode, RequireComponent(typeof(SpriteRenderer))]
#endif
public class Billboard : MonoBehaviour
{
    #region Methods
    private void Start()
    {
        doNotOptimize = true;
        GetComponent<SpriteRenderer>().flipX = false;

        if (applyRandomZRotation)
        {
            randomZRotation = new float[] { 0, -90, 180, 90 }[Random.Range(0, 4)];
        }
    }
    #endregion

    #region Camera Facing Rotation
    private void OnWillRenderObject()
    {
        Camera camera = Camera.current;
        if (camera == null)
        {
            return;
        }

        if (!doNotOptimize)
        {
            updateTimer += Time.deltaTime;
            if (updateTimer < updateInterval)
            {
                return;
            }

            updateInterval = Vector3.Distance(transform.position, camera.transform.position) / UpdateFrequency;
            updateTimer = 0;
        }

        Vector3 eulers = camera.transform.eulerAngles;
        float x = billboardX ? eulers.x : 0f;
        float y = eulers.y;
        float z = applyRandomZRotation ? randomZRotation : 0f;

        if (shaking && Time.timeScale != 0f)
        {
            x += Random.Range(-32f, 32f);
            y += Random.Range(-32f, 32f);
            z += Random.Range(-32f, 32f);
        }

        transform.rotation = Quaternion.Euler(x, y, z);
    }
    #endregion

    #region Inspector Configuration
    [Header("Billboard Settings")]
    public bool doNotOptimize;
    [SerializeField] private bool shaking, billboardX, applyRandomZRotation;
    #endregion

    #region Runtime State and Timing
    private const float UpdateFrequency = 400f;
    private float updateTimer, updateInterval, randomZRotation;
    #endregion
}