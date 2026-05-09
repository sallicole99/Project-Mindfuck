using UnityEngine;
using System.Collections;

public class PrincipalScript : NPC
{
    #region NPC Lifecycle
    public override void OnStart()
    {
        FindingWhatsNeeded();
        base.OnStart();
        Wander();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (seesRuleBreak)
        {
            HandleRuleBreaking();
        }
        else if (timeSeenRuleBreak > 0f)
        {
            timeSeenRuleBreak -= 0.25f * Time.deltaTime;
        }
        if (gauge != null)
        {
            if (officeDoor.lockTime > 0f)
            {
                gauge.Set(maxGaugeLockTime, officeDoor.lockTime);
            }
            if (officeDoor.lockTime <= 0f)
            {
                gauge.Hide();
            }
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (!angry)
        {
            HandlePlayerDetection();

            if (!seesRuleBreak)
            {
                HandleBullyDetection();
            }
        }
        else
        {
            HandlePlayerTargeting();
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!angry && !seesRuleBreak && !inOffice)
            {
                Wander();
            }
        }
    }

    private void FindingWhatsNeeded()
    {
        AudioDevice = GetComponent<AudioSource>();
        audioQueue = GetComponent<AudioQueueScript>();
        cc = FindObjectOfType<CharacterController>();
        baldiScript = FindObjectOfType<BaldiScript>();
        playerScript = FindObjectOfType<PlayerScript>();
        bullyScript = FindObjectOfType<BullyScript>();
    }
    #endregion

    #region Rule and Guilt Handling
    private void HandleRuleBreaking()
    {
        timeSeenRuleBreak += Time.deltaTime;

        float ruleBreakThreshold = playerScript.guiltType == "escape" ? 0.25f : 0.75f;
        if (timeSeenRuleBreak >= ruleBreakThreshold && !angry)
        {
            angry = true;
            seesRuleBreak = false;
            timeSeenRuleBreak = 0f;
            ruleBreakObservationTime = 0f;
            TargetPlayer();
            CorrectPlayer();
        }
    }

    private void HandlePlayerDetection()
    {
        aim = player.position - transform.position;
        if (transform.position.RaycastFromPosition(aim, out hit))
        {
            if (hit.transform.CompareTag("Player") && playerScript.guilt > 0f && !playerScript.alsoInOffice)
            {
                ruleBreakObservationTime += Time.deltaTime;

                if (ruleBreakObservationTime >= 0.2f)
                {
                    seesRuleBreak = true;
                }
            }
            else
            {
                ruleBreakObservationTime = 0f;
            }
        }
        else
        {
            ruleBreakObservationTime = 0f;
        }
    }

    private void HandlePlayerTargeting()
    {
        aim = player.position - transform.position;
        if (transform.position.RaycastFromPosition(aim, out hit))
        {
            if (hit.transform.CompareTag("Player"))
            {
                TargetPlayer();
            }
            else
            {
                LosePlayer();
            }
        }
        else
        {
            LosePlayer();
        }
    }

    private void LosePlayer()
    {
        if (angry && !agent.isStopped)
        {
            WanderWithAnger();
        }
    }

    private void HandleBullyDetection()
    {
        aim = bully.position - transform.position;
        if (transform.position.RaycastFromPosition(aim, out hit, QueryTriggerInteraction.UseGlobal))
        {
            if (hit.transform.name == "Its a Bully" && bullyScript.guilt > 0f && !inOffice)
            {
                TargetBully();
            }
        }
    }
    #endregion

    #region Movement & Navigation
    protected override void Wander(AILocationSelectorScript.LocationType locationType = AILocationSelectorScript.LocationType.Default)
    {
        playerScript.principalBugFixer = 1;
        base.Wander(locationType);
        if (agent.isStopped)
        {
            agent.isStopped = false;
        }
        ResetCooldown();
        if (Random.Range(0f, 10f) <= 1f)
        {
            if (!AudioDevice.isPlaying)
            {
                AudioDevice.PlayOneShot(aud_Whistle);
            }
        }
    }

    private void WanderWithAnger()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Wander();
        }
    }

    protected override void TargetPlayer()
    {
        base.TargetPlayer();
        agent.speed = 26;
    }

    private void TargetBully()
    {
        if (!bullySeen)
        {
            agent.SetDestination(bully.position);
            audioQueue.QueueAudio(audNoBullying);
            agent.speed = 26;
            bullySeen = true;
        }
    }
    #endregion

    #region Detention System
    public IEnumerator CheckTheDoor()
    {
        coolDown = 3;
        agent.speed = 0;
        yield return new WaitForSeconds(2);
        agent.speed = 23;
        onFaculty = true;
    }

    private void CorrectPlayer()
    {
        audioQueue.ClearQueue();
        switch (playerScript.guiltType)
        {
            case "faculty":
                audioQueue.QueueAudio(audNoFaculty);
                break;
            case "running":
                audioQueue.QueueAudio(audNoRunning);
                break;
            case "drink":
                audioQueue.QueueAudio(audNoDrinking);
                break;
            case "escape":
                audioQueue.QueueAudio(audNoEscaping);
                break;
            case "eat":
                audioQueue.QueueAudio(audNoEating);
                break;
            case "bully":
                audioQueue.QueueAudio(audNoBullying);
                break;
        }
    }

    private IEnumerator DragPlayerToDetention(Transform target)
    {
        isDraggingPlayer = true;
        player = target;

        if (target.TryGetComponent<CharacterController>(out var playerCC))
        {
            playerCC.enabled = false;
        }

        agent.SetDestination(point.position);

        while (Vector3.Distance(transform.position, point.position) > 1.5f)
        {
            Vector3 offset = transform.forward * 1.5f;
            Vector3 dragPosition = transform.position + offset;
            dragPosition.y = 4f;
            target.position = dragPosition;
            yield return null;
        }

        if (playerCC != null)
        {
            playerCC.enabled = true;
        }

        isDraggingPlayer = false;
        GiveDetention(target);
    }

    public void GiveDetention(Transform target)
    {
        playerScript.alsoInOffice = true;
        bullyScript.StopPushingPlayer();
        if (playerScript.hugging)
        {
            playerScript.hugging = false;
            playerScript.sweepingFailsave = 0f;
            target.transform.SetParent(null);
        }
        if (playerScript.jumpRope)
        {
            playerScript.jumpRope = false;
            playerScript.DeactivateJumpRope();
            playerScript.playtime.Disappoint();
            target.transform.SetParent(null);
        }
        officeDoor.openTime = 0f;
        inOffice = true;
        playerScript.principalBugFixer = 0;
        agent.isStopped = true;
        cc.enabled = false;
        Vector3 vector = new Vector3(point.position.x, target.position.y, point.position.z);
        cc.enabled = true;
        target.transform.position = vector;
        agent.Warp(vector + target.forward * 10f);
        audioQueue.QueueAudio(audTimes[detentions]);
        audioQueue.QueueAudio(audDetention);
        int num = (int)Random.Range(0f, audScolds.Length);
        audioQueue.QueueAudio(audScolds[num]);
        officeDoor.LockDoor(lockTime[detentions]);
        maxGaugeLockTime = lockTime[detentions];
        if (gauge == null)
        {
            gauge = GaugeManager.Instance.CreateGaugeInstance(gaugeDetentionSprite, maxGaugeLockTime);
        }
        if (baldiScript.isActiveAndEnabled)
        {
            baldiScript.Hear(transform.position, 9.5f);
        }
        if (officeDoor.lockTime <= 99f)
        {
            coolDown = 3f;
        }
        else
        {
            coolDown = 10f;
        }
        angry = false;
        seesRuleBreak = false;
        detentions++;
        if (detentions > lockTime.Length - 1)
        {
            detentions = lockTime.Length - 1;
        }
        playerScript.guilt = 0f;
        StartCoroutine(QuickDelay());
    }

    public IEnumerator QuickDelay()
    {
        yield return new WaitForSeconds(3.5f);
        agent.isStopped = false;
        inOffice = false;
        Wander();
    }
    #endregion

    #region Trigger Events
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player" & summon)
        {
            summon = false;
            agent.speed = 23f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name == "OfficeTrigger")
        {
            inOffice = true;
        }
        if (other.CompareTag("Player") && angry)
        {
            if (AdditionalGameCustomizer.Instance.DragToDetention && !isDraggingPlayer)
            {
                StartCoroutine(DragPlayerToDetention(other.transform));
            }
            else if (!AdditionalGameCustomizer.Instance.DragToDetention)
            {
                GiveDetention(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "OfficeTrigger")
        {
            inOffice = false;
        }
        if (other.name == "Bully")
        {
            bullySeen = false;
        }
    }
    #endregion

    #region Serialized Field States
    [Header("References")]
    [SerializeField] private Sprite gaugeDetentionSprite;
    [SerializeField] private Transform bully, point;
    [SerializeField] private DoorScript officeDoor;

    [Header("Audio and Feedback")]
    [SerializeField] private AudioClip[] audTimes;
    [SerializeField] private AudioClip[] audScolds;
    [SerializeField] private AudioClip audDetention, audNoDrinking, audNoEating, audNoBullying, audNoFaculty, audNoRunning, audNoEscaping, aud_Whistle;
    #endregion

    #region Internal References
    private CharacterController cc;
    private BaldiScript baldiScript;
    private PlayerScript playerScript;
    private BullyScript bullyScript;
    [HideInInspector] public bool angry, onFaculty;
    private AudioQueueScript audioQueue;
    private int detentions;
    private float maxGaugeLockTime, ruleBreakObservationTime, timeSeenRuleBreak;
    private int[] lockTime = { 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 99 };
    private AudioSource AudioDevice;
    private bool summon, seesRuleBreak, bullySeen, inOffice;
    private RaycastHit hit;
    private Vector3 aim;
    private Gauge gauge;
    private bool isDraggingPlayer = false;
    #endregion
}