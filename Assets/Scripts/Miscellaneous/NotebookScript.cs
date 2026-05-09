using UnityEngine;

public class NotebookScript : MonoBehaviour
{
    #region Unity Lifecycle
    private void Start() => InitializeComponents();

    private void Update()
    {
        HandleRespawn();
        NotebookInteraction();
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        respawnTime = 120f;

        audioDevice = GetComponent<AudioSource>();
        lgm = FindObjectOfType<LearningGameManager>();
        gc = FindObjectOfType<GameControllerScript>();
        notebookSprite = GetComponentInChildren<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        up = true;

        if (AdditionalGameCustomizer.Instance != null && AdditionalGameCustomizer.Instance.RandomizeBookColor)
        {
            notebookSprite.sprite = AdditionalGameCustomizer.Instance.BookColors[Random.Range(0, AdditionalGameCustomizer.Instance.BookColors.Length)];
        }
    }
    #endregion

    #region Notebook Respawn Logic
    private void HandleRespawn()
    {
        if (gc.mode != "endless") return;

        if (!transform.IsWithinDistanceFrom(player, 60f) && respawnTime.CountdownWithDeltaTime() == 0 && !up)
        {
            RespawnNotebook();
        }
    }

    private void RespawnNotebook()
    {
        transform.position = new Vector3(transform.position.x, 4f, transform.position.z);
        up = true;
        audioDevice.Play();
    }
    #endregion

    #region Notebook Interaction Logic
    private void NotebookInteraction()
    {
        if (Input.GetMouseButtonDown(0) && Time.timeScale != 0f && transform.IsWithinDistanceFrom(player, 10f))
        {
            if (Sych.ScreenCenterRaycast(out RaycastHit hit) && hit.transform.CompareTag("Notebook"))
            {
                CollectNotebook();
            }
        }
    }

    private void CollectNotebook()
    {
        transform.position = new Vector3(transform.position.x, -20f, transform.position.z);
        up = false;
        respawnTime = 120f;
        gc.CollectNotebook(1);

        if (AdditionalGameCustomizer.Instance?.NoYCTP == true)
        {
            NoYCTPMode();
        }
        else
        {
            StartLearningGame();
        }
    }
    #endregion

    #region No YCTP Mode Logic
    private void NoYCTPMode()
    {
        gc.Icon.Rebind();
        gc.Icon.Play("IconSpin", -1, 0f);
        audioDevice.PlayClip(gc.aud_Collected, false, 1f);

        if (gc.player.stamina < 100f)
        {
            gc.player.SetStamina(PlayerScript.StaminaChangeMode.Set, 100f);
        }

        if (gc.notebooks == 1 && !gc.spoopMode)
        {
            lgm.Tutor.tutorSource.Stop();
            lgm.quarter.SetActive(true);
            lgm.Tutor.tutorSource.PlayClip(lgm.aud_Prize, false, 1f);
        }

        if (gc.notebooks == 2)
        {
            gc.ActivateSpoopMode();
        }

        if (gc.spoopMode)
        {
            gc.baldiScrpt.GetAngry(1f);
        }

        if (gc.notebooks == gc.maxNotebooks && gc.mode == "story")
        {
            TriggerFinalSequence();

            if (AdditionalGameCustomizer.Instance?.ExitCounter == true)
            {
                gc.Icon.Rebind();
                gc.Icon.Play("Icon2Idle", -1, 0f);
            }
        }
    }

    private void TriggerFinalSequence()
    {
        if (AdditionalGameCustomizer.Instance?.FinalModeTV == true)
        {
            StartCoroutine(lgm.Television.StartTVSequence(lgm.aud_AllNotebooks));
        }
        else
        {
            gc.audioDevice.PlayClip(lgm.aud_AllNotebooks, false, 0.8f);
        }

        gc.escapeMusic.clip = gc.SchoolhouseEscape;
        gc.escapeMusic.loop = true;
        gc.escapeMusic.Play();
    }
    #endregion

    #region Learning Game Launch
    private void StartLearningGame()
    {
        GameObject game = Instantiate(GameControllerScript.Instance.learningGame);
        var mathGame = game.GetComponent<MathGameScript>();
        mathGame.playerPosition = player.position;
    }
    #endregion

    #region Fields
    private float respawnTime;
    private bool up;
    private AudioSource audioDevice;
    private GameControllerScript gc;
    private LearningGameManager lgm;
    private Transform player;
    private SpriteRenderer notebookSprite;
    #endregion
}