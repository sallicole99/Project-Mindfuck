using UnityEngine;

public class CraftersScript : NPC
{
    #region Initialization
    public override void OnStart()
    {
        base.OnStart();
        gc = FindObjectOfType<GameControllerScript>();
        cc = FindObjectOfType<CharacterController>();
        audioDevice = GetComponent<AudioSource>();
        normalSprite = spriteImage.sprite;
        baseSpeed = agent.speed;
    }
    #endregion

    #region Update & State Logic
    public override void OnUpdate()
    {
        if (forceShowTime > 0f)
        {
            forceShowTime -= Time.deltaTime;
        }

        if (angerCooldown > 0f)
        {
            angerCooldown -= Time.deltaTime;
        }

        if (gettingAngry)
        {
            anger += Time.deltaTime;
            if (anger >= 1f & !angry)
            {
                angry = true;
                audioDevice.PlayOneShot(aud_Intro);
                spriteImage.sprite = angrySprite;
            }
        }
        else if (anger > 0f)
        {
            anger -= Time.deltaTime;
        }

        if (!angry)
        {
            if (((transform.position - agent.destination).magnitude <= 20f &
                (transform.position - player.position).magnitude >= 60f) || forceShowTime > 0f)
            {
                spriteImage.sprite = normalSprite;
            }
            else
            {
                spriteImage.sprite = invisibleSprite;
            }
        }
        else
        {
            if (AdditionalGameCustomizer.Instance != null && AdditionalGameCustomizer.Instance.SkipCraftersAttack)
            {
                agent.speed += 60f * Time.deltaTime;
            }
            else
            {
                if (agent.speed < 45f)
                {
                    agent.speed += 10f * Time.deltaTime;
                }
            }

            TargetPlayer();

            if (!audioDevice.isPlaying)
            {
                audioDevice.PlayOneShot(aud_Loop);
            }
        }
    }
    #endregion

    #region Visibility & Aggression Check
    public override void OnFixedUpdate()
    {
        if (gc.notebooks >= AngerLimit)
        {
            CheckForPlayerAndVisibility();
        }
    }

    private void CheckForPlayerAndVisibility()
    {
        if ((transform.position + Vector3.up * 2f).RaycastFromPosition(player.position - transform.position, out RaycastHit raycastHit))
        {
            if (raycastHit.transform.CompareTag("Player") && craftersRenderer.isVisible && spriteImage.sprite == normalSprite)
            {
                gettingAngry = true;
            }
            else
            {
                gettingAngry = false;
            }
        }
    }
    #endregion

    #region Movement & Targeting
    protected override void HandleMovement() { }

    protected override void TargetPlayer() => agent.SetDestination(player.position);

    public void GiveLocation(Vector3 location, bool flee)
    {
        if (!angry && agent.isActiveAndEnabled)
        {
            agent.SetDestination(location);
            if (flee)
            {
                forceShowTime = 3f;
            }
        }
    }
    #endregion

    #region Collision & Attack Trigger
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") & angry)
        {
            if (AdditionalGameCustomizer.Instance != null && AdditionalGameCustomizer.Instance.SkipCraftersAttack)
            {
                GiveConsequence();
            }
            else
            {
                GameObject attacker = Instantiate(attackingCrafters);
                attacker.transform.position = transform.position + Vector3.up * 4f;
                attacker.GetComponent<CraftersAttackerScript>().playerTransform = player;
                attacker.GetComponent<CraftersAttackerScript>().crafters = gameObject;
                attacker.GetComponent<CraftersAttackerScript>().craftersScript = this;
                attacker.GetComponent<CraftersAttackerScript>().Attack();
                gameObject.SetActive(false);
            }
        }
    }

    public void GiveConsequence()
    {
        cc.enabled = true;
        gc.CraftersTeleport();

        if (!endless)
        {
            gameObject.SetActive(false);
        }
        else
        {
            angerCooldown = 3f;
            anger = 0f;
            gettingAngry = false;
            angry = false;
            agent.speed = baseSpeed;
            spriteImage.sprite = normalSprite;
            audioDevice.Stop();
        }
    }
    #endregion

    #region Serialized Inspector Variables
    [Header("States & Conditions")]
    [SerializeField] private float anger;
    [SerializeField] private bool angry, gettingAngry;

    [Header("References & Components")]
    [SerializeField] private Renderer craftersRenderer;
    [SerializeField] private SpriteRenderer spriteImage;

    [Header("Audio & Visuals")]
    [SerializeField] private AudioClip aud_Intro;
    [SerializeField] private AudioClip aud_Loop;
    [SerializeField] private Sprite angrySprite, normalSprite, invisibleSprite;

    [Header("Movement & Speed")]
    [SerializeField] private GameObject attackingCrafters;
    [SerializeField] private float baseSpeed, angerCooldown;

    [Header("Gameplay Flow")]
    [SerializeField] private int AngerLimit;
    public bool endless;
    #endregion

    #region Internal State
    private CharacterController cc;
    private GameControllerScript gc;
    private float forceShowTime;
    private AudioSource audioDevice;
    #endregion
}