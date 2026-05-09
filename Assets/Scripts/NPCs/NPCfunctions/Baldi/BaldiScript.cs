using UnityEngine;
using System.Collections;

public class BaldiScript : NPC
{
    #region Unity Lifecycle
    public override void OnStart()
    {
        base.OnStart();
        baldiAudio = GetComponent<AudioSource>();
        GetAngry(0f);
        Move();

		if (endless)
		{
			Endless();
		}

        Wander();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

		if (baldiTempAnger > 0f)
		{
			baldiTempAnger -= 0.02f * Time.deltaTime;
		}
		else
		{
			baldiTempAnger = 0f;
		}
    }

    public override void OnFixedUpdate()
    {
        if (player == null) return;

        if ((transform.position + Vector3.up * 2f).RaycastFromPosition(player.position - transform.position, out RaycastHit raycastHit) &&
            raycastHit.transform.CompareTag("Player"))
        {
            TargetPlayer();
        }
    }
    #endregion

    #region Movement
    protected override void Wander(AILocationSelectorScript.LocationType locationType = AILocationSelectorScript.LocationType.Default)
    {
        base.Wander(locationType);
        currentPriority = 0f;
    }

    protected override void TargetPlayer()
    {
        base.TargetPlayer();
        currentPriority = 0f;
    }

	public void Move()
	{
		agent.speed = speed;
		baldiAudio.PlayOneShot(slap);
		baldiAnimator.SetTrigger("slap");

		if (!stopMoving)
		{
			Invoke(nameof(OnMoveDone), timeToMove);
		}
    }

	private void OnMoveDone()
	{
		agent.speed = 0;

		if (agent.remainingDistance <= 0.1f)
		{
			Wander();
		}

		if (!stopMoving)
		{
			Invoke(nameof(Move), baldiWait);
		}
    }
    #endregion

    #region Anger System
    public void GetAngry(float value)
    {
        baldiAnger += value;

		if (baldiAnger < 0.5f)
		{
			baldiAnger = 0.5f;
		}

        baldiWait = -3f * baldiAnger / (baldiAnger + 2f / baldiSpeedScale) + 3f;
    }

    public void GetTempAngry(float value) => baldiTempAnger += value;

    public void Endless()
    {
        Invoke(nameof(Endless), timeToAnger);
        timeToAnger = angerFrequency;
        GetAngry(angerRate);
        angerRate += angerRateRate;
    }
    #endregion

    #region Hearing Detection
    public void Hear(Vector3 soundLocation, float priority)
    {
        if (!isActiveAndEnabled) return;

        bool canHear = !antiHearing && priority >= currentPriority;
        bool inNoSqueeArea = false;

        foreach (Collider collider in Physics.OverlapSphere(soundLocation, 0.1f))
        {
            if (collider.gameObject.CompareTag("NoSquee Area"))
            {
                canHear = false;
                inNoSqueeArea = true;
                break;
            }
        }

        if (canHear)
        {
            agent.SetDestination(soundLocation);
            currentPriority = priority;

            if (!inNoSqueeArea && AdditionalGameCustomizer.Instance.Indicator)
            {
                baldicator.Rebind();
                baldicator.Play("Indicator_Heared", -1, 0f);
            }
        }
        else
        {
            if (!inNoSqueeArea && AdditionalGameCustomizer.Instance.Indicator)
            {
                baldicator.Rebind();
                baldicator.Play("Indicator_Confused", -1, 0f);
            }
        }
    }

    public void ActivateAntiHearing(float SetTime) => StartCoroutine(SetHearingTimer(SetTime));

    private IEnumerator SetHearingTimer(float Timer)
    {
        Wander();
        antiHearing = true;
        yield return new WaitForSeconds(Timer);
        antiHearing = false;
    }
    #endregion

    #region Serialized Field States
    [Header("Baldi's Stats")]
    [SerializeField] private float baldiAnger;
    [SerializeField] private float baldiTempAnger, baldiWait, baldiSpeedScale;

    [Header("Movement and Behavior")]
    [SerializeField] private float speed;
    [SerializeField] private float timeToMove;
    public bool stopMoving, antiHearing;

    [Header("Anger Management")]
    [SerializeField] private float angerRate;
    [SerializeField] private float angerRateRate, angerFrequency, timeToAnger;
    public bool endless;

    [Header("Audio and Animation")]
    [SerializeField] private AudioClip slap;
    [SerializeField] private Animator baldicator, baldiAnimator;

    private float currentPriority;
    private AudioSource baldiAudio;
    #endregion
}