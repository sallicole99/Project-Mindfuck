using UnityEngine;
using UnityEngine.AI;

public static class Sych
{
    public static bool ScreenRaycastMatchesTagAndTransform(this Transform target, string tag, out RaycastHit hit, float maxDistance) => ScreenCenterRaycast(out hit) && hit.transform == target && target.IsWithinDistance(maxDistance) && hit.transform.CompareTag(tag);

    public static bool ScreenRaycastMatchesTag(string tag, out RaycastHit hit, float maxDistance) => ScreenCenterRaycast(out hit) && hit.transform.IsWithinDistance(maxDistance) && hit.transform.CompareTag(tag);

    public static bool ScreenRaycastMatchesCollider(this Collider col, out RaycastHit hit, float maxDistance) => ScreenCenterRaycast(out hit) && hit.transform.IsWithinDistance(maxDistance) && hit.collider == col;

    public static bool RaycastFromPosition(this Vector3 origin, Vector3 direction, out RaycastHit hit, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) => Physics.Raycast(origin, direction, out hit, Mathf.Infinity, -5, triggerInteraction);

    public static bool RaycastFromPositionWithDistance(this Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = Mathf.Infinity) => Physics.Raycast(origin, direction, out hit, maxDistance, -5, QueryTriggerInteraction.Ignore);

    public static bool ScreenCenterRaycast(out RaycastHit hit) => Physics.Raycast(Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2)), out hit);

    public static bool IsWithinDistance(this Transform t, float maxDistance) => Vector3.Distance(Camera.main.transform.position, t.position) <= maxDistance;

    public static int GetRoundedRandomInRange(float min, float max) => Mathf.RoundToInt(Random.Range(min - 0.4f, max - 0.5f));

    public static T GetComponentByTag<T>(string tag) where T : MonoBehaviour => GameObject.FindGameObjectWithTag(tag).GetComponent<T>();

    public static float CountdownWithDeltaTime(this ref float timer, float incrementAmount = 1) => timer = Mathf.Max(0, timer - incrementAmount * Time.deltaTime);

    public static float CountdownWithUnscaledDeltaTime(this ref float timer) => timer = Mathf.Max(0, timer - Time.unscaledDeltaTime);

    public static float IncrementOverTime(this ref float value, float incrementAmount) => value += incrementAmount * Time.deltaTime;

    public static bool IsWithinDistanceFrom(this Transform t, Transform reference, float maxDistance) => Vector3.Distance(reference.position, t.position) <= maxDistance;

    public static bool IsReadyToMove(this NavMeshAgent agent) => !agent.isStopped && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

    public static void DestroyIfExists(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null) Object.DestroyImmediate(obj);
    }

    public static void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public static void PlayClip(this AudioSource audioSource, AudioClip clip, bool loop, float volume)
    {
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = volume;
        audioSource.Play();
    }

    public static Vector3 SnapToGrid(Vector3 position)
    {
        float gridSize = 10f;

        float centerX = Mathf.Floor(position.x / gridSize) * gridSize;
        float centerZ = Mathf.Floor(position.z / gridSize) * gridSize;

        float snappedX = centerX + (gridSize / 2);
        float snappedZ = centerZ + (gridSize / 2);

        return new Vector3(snappedX, 5, snappedZ);
    }
}