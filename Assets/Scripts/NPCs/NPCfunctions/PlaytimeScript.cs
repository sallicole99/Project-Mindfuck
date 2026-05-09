using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PlaytimeScript : NPC
{
    #region Unity Lifecycle
    public override void OnStart()
    {
        base.OnStart();
        ps = FindObjectOfType<PlayerScript>();
        agent.speed = walkSpeed;
        canTargetPlayer = true;
        audioDevice = GetComponent<AudioSource>();
        Wander(AILocationSelectorScript.LocationType.Hall);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (ps.jumpRope && Vector3.Distance(transform.position, ps.transform.position) >= 45f)
        {
            ps.DeactivateJumpRope();
            Disappoint();
        }

        if (playCool >= 0f)
        {
            playCool -= Time.deltaTime;
        }
        else if (animator.GetBool("disappointed") && !ps.jumpRope)
        {
            playCool = 0f;
            animator.SetBool("disappointed", false);
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (!ps.jumpRope)
        {
            if (!playerSeen && agent.velocity.magnitude <= 1f && coolDown <= 0f)
            {
                Wander(AILocationSelectorScript.LocationType.Hall);
            }
            jumpRopeStarted = false;
        }
        else if (!jumpRopeStarted)
        {
            if (playingRoutine != null)
            {
                StopCoroutine(playingRoutine);
            }
            playingRoutine = StartCoroutine(StartPlaying());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !ps.jumpRope)
        {
            Collider component = GetComponent<Collider>();
            component.enabled = false;
            component.enabled = true;
        }
    }
    #endregion

    #region Player Detection & Targeting
    protected override void CheckForPlayer()
    {
        if (transform.position.RaycastFromPositionWithDistance(player.position - transform.position, out RaycastHit raycastHit, 30f))
        {
            bool playerInRange = (transform.position - player.position).magnitude <= 30f;

            if (raycastHit.transform.CompareTag("Player") && playerInRange && playCool <= 0f)
            {
                playerSeen = true;
                TargetPlayer();
            }
            else if (playerSeen && coolDown <= 0f)
            {
                playerSeen = false;
                Opposition();
                agent.speed = walkSpeed;
                Wander(AILocationSelectorScript.LocationType.Hall);
            }
        }
    }

    protected override void TargetPlayer()
    {
        animator.SetBool("disappointed", false);
        agent.SetDestination(player.position);
        agent.stoppingDistance = 2f;
        agent.angularSpeed = 200f;
        agent.updateRotation = true;

        agent.speed = runSpeed;
        coolDown = 0.2f;

        if (!playerSpotted)
        {
            playerSpotted = true;
            if (!audioDevice.isPlaying)
            {
                audioDevice.PlayOneShot(aud_LetsPlay);
            }
        }
    }

    private void Opposition()
    {
        Vector3 directionAway = transform.position - (player.position - transform.position).normalized * 500f;
        if (NavMesh.SamplePosition(directionAway, out NavMeshHit navMeshHit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(navMeshHit.position);
        }
    }
    #endregion

    #region Wandering
    protected override void Wander(AILocationSelectorScript.LocationType locationType)
    {
        if (ps.jumpRope) return;

        base.Wander(AILocationSelectorScript.LocationType.Hall);

        agent.stoppingDistance = 1f;
        agent.angularSpeed = 180f;
        agent.updateRotation = true;
        agent.speed = walkSpeed;

        playerSpotted = false;

        int audVal = Random.Range(0, aud_Random.Length);
        if (!audioDevice.isPlaying)
        {
            audioDevice.PlayOneShot(aud_Random[audVal]);
        }

        ResetCooldown();
    }

    public void ResumeWandering()
    {
        if (!ps.jumpRope)
        {
            agent.speed = walkSpeed;
            playingRoutine = null;
            Wander(AILocationSelectorScript.LocationType.Hall);
        }
    }
    #endregion

    #region Jump Rope State Handling
    private IEnumerator StartPlaying()
    {
        ps.isForcedToLook = true;
        canTargetPlayer = false;
        jumpRopeStarted = true;

        Vector3 moveBackPosition = transform.position - transform.forward * 10f;
        agent.SetDestination(moveBackPosition);
        agent.speed = 16f;

        while (Vector3.Distance(transform.position, moveBackPosition) > 1f)
        {
            yield return null;
        }

        agent.speed = 0f;
        playingRoutine = null;
    }

    public void Disappoint()
    {
        if (!animator.GetBool("disappointed"))
        {
            if (playingRoutine != null)
            {
                StopCoroutine(playingRoutine);
                playingRoutine = null;
            }

            Wander(AILocationSelectorScript.LocationType.Hall);
            agent.speed = walkSpeed;
            animator.SetBool("disappointed", true);

            canTargetPlayer = true;
            jumpRopeStarted = false;
            playCool = 15f;

            audioDevice.Stop();
            audioDevice.PlayOneShot(aud_Sad);
        }
    }
    #endregion

    #region Movement Override
    protected override void HandleMovement()
    {
        if (ps.jumpRope) return;
        base.HandleMovement();
    }
    #endregion

    #region Serialized Field States
    [Header("Audio and Animations")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip[] aud_Random = new AudioClip[2];
    public AudioClip[] aud_Numbers = new AudioClip[10];
    [SerializeField] private AudioClip aud_LetsPlay, aud_Sad;
    public AudioClip aud_Congrats, aud_ReadyGo, aud_Oops;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 15f;
    [SerializeField] private float runSpeed = 20f;

    private PlayerScript ps;
    [HideInInspector] public AudioSource audioDevice;
    [HideInInspector] public float playCool;
    private Coroutine playingRoutine;
    private bool playerSeen, playerSpotted;
    [HideInInspector] public bool jumpRopeStarted;
    #endregion
}