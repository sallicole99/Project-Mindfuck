using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    #region Unity Lifecycle
    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        OnStart();
    }

    protected virtual void Update() => OnUpdate();

    protected virtual void FixedUpdate()
    {
        if (canTargetPlayer)
        {
            CheckForPlayer();
        }

        if (!isInteracting)
        {
            HandleMovement();
        }

        OnFixedUpdate();
    }
    #endregion

    #region AI Behavior
    protected virtual void CheckForPlayer()
    {
        Vector3 direction = player.position - transform.position;

        if (transform.position.RaycastFromPosition(direction, out RaycastHit hit))
        {
            isInteracting = hit.transform.CompareTag("Player");

            if (isInteracting && canTargetPlayer)
            {
                TargetPlayer();
            }
        }
    }

    protected virtual void HandleMovement()
    {
        if (!agent.IsReadyToMove() || coolDown.CountdownWithDeltaTime() != 0) return;

        if (!canTargetPlayer || !isInteracting)
        {
            Wander();
        }
    }

    protected virtual void Wander(AILocationSelectorScript.LocationType locationType = AILocationSelectorScript.LocationType.Default)
    {
        wanderer?.SetNewTargetForAgent(agent, locationType);
        ResetCooldown();
    }

    protected virtual void TargetPlayer()
    {
        agent.SetDestination(player.position);
        ResetCooldown();
    }
    #endregion

    #region Hooks
    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
    public virtual void OnFixedUpdate() { }
    #endregion

    #region Utility
    protected void ResetCooldown() => coolDown = 1;
    #endregion

    #region Editor Gizmos
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent == null || !agent.hasPath || agent.path.corners == null || agent.path.corners.Length < 2)
        {
            return;
        }

        Gizmos.color = pathColor;
        Vector3[] corners = agent.path.corners;

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Gizmos.DrawLine(corners[i], corners[i + 1]);
            Gizmos.DrawSphere(corners[i], 0.1f);
        }
    }
#endif
    #endregion

    #region Serialized Fields
    [Header("NPC Functions")]
    [SerializeField] private Color pathColor = Color.red;
    [SerializeField] protected Transform player;
    [SerializeField] protected AILocationSelectorScript wanderer;
    public bool isInteracting, canTargetPlayer;
    #endregion

    #region Internal State
    protected float coolDown;
    protected NavMeshAgent agent;
    #endregion
}