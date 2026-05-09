using UnityEngine;
using UnityEngine.AI;

public class AILocationSelectorScript : MonoBehaviour
{
    #region Unity Lifecycle
    private void Awake() => ambience = FindObjectOfType<AmbienceScript>();
    #endregion

    #region Setting Locations
    public Vector3 SetNewTargetForAgent(NavMeshAgent agent, LocationType locationType)
    {
        if (ambience == null)
        {
            ambience = FindObjectOfType<AmbienceScript>();
        }

        ambience?.PlayAudio();

        Transform[] targetLocations = locationType switch
        {
            LocationType.Hall => hallLocation,
            LocationType.Room => roomLocation,
            LocationType.Present => presentLocation,
            _ => newLocation
        };

        int id = Random.Range(0, targetLocations.Length);
        Vector3 targetPosition = targetLocations[id].position;

        if (agent != null)
        {
            agent.SetDestination(targetPosition);
        }

        return targetPosition;
    }
    #endregion

    #region Serialized Fields
    [Header("Location Arrays")]
    [SerializeField] private Transform[] newLocation;
    [SerializeField] private Transform[] hallLocation, roomLocation, presentLocation;

    private AmbienceScript ambience;
    public enum LocationType
    {
        Default,
        Hall,
        Room,
        Present
    }
    #endregion
}