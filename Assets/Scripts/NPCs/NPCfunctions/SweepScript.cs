using UnityEngine;

public class SweepScript : NPC
{
    #region Initialization and Setup
    public override void OnStart()
    {
        audioDevice = GetComponent<AudioSource>();
        waitTime = Random.Range(60f, 180f);
    }
    #endregion

    #region Activity and Timer Logic
    public override void OnUpdate()
    {
        base.OnUpdate();

        if (waitTime > 0f)
        {
            waitTime -= Time.deltaTime;
            return;
        }

        if (!active)
        {
            active = true;
            activeTime = Random.Range(30f, 60f);
            Wander(AILocationSelectorScript.LocationType.Default);
            audioDevice.PlayOneShot(aud_Intro);
            return;
        }

        if (active)
        {
            activeTime -= Time.deltaTime;

            if (activeTime <= 0f)
            {
                GoHome();
            }
        }
    }
    #endregion

    #region Movement Handling
    protected override void HandleMovement()
    {
        if (waitTime <= 0f && active)
        {
            base.HandleMovement();
        }
    }

    public override void OnFixedUpdate()
    {
        if (waitTime > 0f) return;

        if (active && activeTime > 0f)
        {
            if (agent.remainingDistance <= 1f && !agent.pathPending && coolDown <= 0f)
            {
                base.Wander();
            }
        }
    }

    private void GoHome()
    {
        active = false;
        agent.SetDestination(homeLocation.position);
        waitTime = Random.Range(60f, 180f);
    }
    #endregion

    #region Collision Interaction
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC") || other.CompareTag("Player"))
        {
            if (!audioDevice.isPlaying)
            {
                audioDevice.PlayOneShot(aud_Sweep);
            }
            if (other.transform.name == "Its a Bully")
            {
                base.Wander();
            }
        }
    }
    #endregion

    #region Serialized Fields
    [Header("Movement and Navigation")]
    [SerializeField] private Transform homeLocation;
    [SerializeField] private float waitTime, activeTime;

    [Header("Audio")]
    [SerializeField] private AudioClip aud_Sweep;
    [SerializeField] private AudioClip aud_Intro;

    [Header("State Management")]
    [SerializeField] private bool active;
    #endregion

    #region Internal References
    private AudioSource audioDevice;
    #endregion
}