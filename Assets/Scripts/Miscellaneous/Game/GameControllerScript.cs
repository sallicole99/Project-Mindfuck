using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameControllerScript : MonoBehaviour
{
    #region SingletonSetup
    private void Awake() => Instance = this;
    public static GameControllerScript Instance;
    #endregion

    #region UnityCallbacks
    private void Start()
    {
        InitializeGameSettings();
        UpdateNotebookCount();
    }

    private void Update()
    {
        if (!KF.gamePaused)
        {
            FinaleModeAnnoyance();
        }

        GameOverFunction();
    }
    #endregion

    #region Initialization
    private void InitializeGameSettings()
    {
        Singleton<Options>.Instance.GetVolume();
        Singleton<Options>.Instance.GetVSync();

        Time.timeScale = 1f;
        AudioListener.pause = false;
        cullingMask = PlayerCamera.cullingMask;

        audioQueue = GetComponent<AudioQueueScript>();
        Math = GetComponent<LearningGameManager>();
        progress = GetComponent<EndingManager>();

        mode = PlayerPrefs.GetString("CurrentMode");

        if (mode == "endless")
        {
            baldiScrpt.endless = true;
            sockCript.endless = true;
        }

        schoolMusic.Play();
    }

    public void Fliparoo()
    {
        player.height = 6f;
        player.fliparoo = 180f;
        player.flipaturn = -1f;
        Camera.main.GetComponent<CameraScript>().offset = new Vector3(0f, -1f, 0f);
    }
    #endregion

    #region NotebookManagement
    public void UpdateNotebookCount()
    {
        notebookCount.text = mode == "story" ? $"{notebooks}/{maxNotebooks}" : $"{notebooks}";

        if (mode == "endless" && notebooks / maxNotebooks > lastRespawnCount)
        {
            lastRespawnCount = notebooks / maxNotebooks;
            EndlessModeRestart();
        }

        if (notebooks == maxNotebooks && mode == "story" && !finaleMode)
        {
            ActivateFinaleMode();
        }

        if (AdditionalGameCustomizer.Instance?.ExitCounter == true && notebooks == maxNotebooks)
        {
            notebookCount.text = $"{exitsReached}/4";
        }
    }

    public void CollectNotebook(float numberOfNotebooks)
    {
        notebooks += Mathf.FloorToInt(numberOfNotebooks);
        UpdateNotebookCount();
        if (ChaosMode)
        {
            foreach (GameObject obj in ObjectsToEnable)
            {
                if (obj != null)
                {
                    GameObject clone = Instantiate(obj, obj.transform.position, obj.transform.rotation);
                    clone.name = obj.name;
                    clone.SetActive(true);
                }
            }
        }
    }
    #endregion

    #region HelperFunctions
    private void EndlessModeRestart()
    {
        ItemsToRespawn.ForEach(item => item.SetActive(true));
        MachinesToRestock.ForEach(machine => machine?.RestockVendingMachine());
    }
    #endregion

    #region SpoopModeHandling
    public void GetAngry(float value)
    {
        if (!spoopMode)
        {
            ActivateSpoopMode();
        }
        baldiScrpt.GetAngry(value);
    }

    public void ActivateSpoopMode()
    {
        spoopMode = true;

        entrances.ForEach(e => e.Enable());
        ObjectsToDisable.ForEach(o => o.SetActive(false));
        ObjectsToEnable.ForEach(o => o.SetActive(true));

        schoolMusic.Stop();
        Math.learnMusic.Stop();

        if (AdditionalGameCustomizer.Instance != null && !AdditionalGameCustomizer.Instance.NoYCTP)
        {
            Math.learnMusic.PlayOneShot(aud_Hang);
        }
        else
        {
            StartCoroutine(audioQueue.FadeOut(schoolMusic, 0.25f));
        }
    }
    #endregion

    #region GameOverLogic
    private void GameOverFunction()
    {
        if (!player.gameOver) return;

        AudioListener.pause = true;
        gamaOvarDevice.ignoreListenerPause = true;
        Time.timeScale = 0f;

        PlayerCamera.farClipPlane = gameOverDelay * 400f;
        gameOverDelay -= Time.unscaledDeltaTime;

        if (!gamaOvarDevice.isPlaying)
        {
            audOverVal = (int)Random.Range(0f, LoseSounds.Length);
            gamaOvarDevice.PlayOneShot(LoseSounds[audOverVal]);
        }

        if (mode == "endless" && notebooks > PlayerPrefs.GetInt("HighBooks") && !highScoreText.activeSelf)
        {
            highScoreText.SetActive(true);
        }

        if (gameOverDelay <= 0f)
        {
            if (mode == "endless")
            {
                if (notebooks > PlayerPrefs.GetInt("HighBooks"))
                {
                    PlayerPrefs.SetInt("HighBooks", notebooks);
                }
                PlayerPrefs.SetInt("CurrentBooks", notebooks);
            }
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameoverScene);
        }
    }
    #endregion

    #region FinaleModeManagement
    private void ActivateFinaleMode()
    {
        finaleMode = true;
        entrances.ForEach(exits => exits.Disable());
    }

    private void FinaleModeAnnoyance()
    {
        if (!finaleMode || audioDevice.isPlaying) return;

        if (exitsReached == 2)
        {
            PlayAudioClip(aud_ChaosStartLoop, true);
        }
        else if (exitsReached == 3 && !progress.GetSecret & !progress.GetResults)
        {
            PlayAudioClip(aud_ChaosFinal, true);
        }
    }

    private void PlayAudioClip(AudioClip clip, bool loop)
    {
        audioDevice.clip = clip;
        audioDevice.loop = loop;
        audioDevice.Play();
    }

    private IEnumerator SchoolEscapeMusic()
    {
        float volume = 1f;
        while (volume > 0f)
        {
            volume -= 3f * Time.deltaTime;
            escapeMusic.volume = volume;
            yield return null;
        }
        escapeMusic.volume = 0f;
        escapeMusic.enabled = false;
        yield return new WaitForSeconds(1.5f);
        escapeMusic.clip = Slowed_SchoolhouseEscape;
        escapeMusic.enabled = true;
        escapeMusic.volume = 1f;
        escapeMusic.Play();
        yield break;
    }
    #endregion

    #region ExitCounterHandling
    public void ExitReached()
    {
        exitsReached++;

        if (AdditionalGameCustomizer.Instance != null && AdditionalGameCustomizer.Instance.ExitCounter)
        {
            UpdateNotebookCount();
            Icon.Rebind();
            Icon.Play("Icon2Spin", -1, 0f);
        }

        if (exitEasingCoroutine != null)
        {
            StopCoroutine(exitEasingCoroutine);
        }
        exitEasingCoroutine = StartCoroutine(exitEasing(exitsReached));

        if (exitsReached == 1)
        {
            audioDevice.PlayOneShot(aud_Switch, 0.8f);
            if (AdditionalGameCustomizer.Instance != null)
            {
                switch (AdditionalGameCustomizer.Instance.currentSkybox)
                {
                    case AdditionalGameCustomizer.SkyboxStyle.Day:
                        RenderSettings.skybox = AdditionalGameCustomizer.Instance.NormalRedSky;
                        break;
                    case AdditionalGameCustomizer.SkyboxStyle.Sunset:
                        RenderSettings.skybox = AdditionalGameCustomizer.Instance.RedTwilightSky;
                        break;
                    case AdditionalGameCustomizer.SkyboxStyle.Night:
                        RenderSettings.skybox = AdditionalGameCustomizer.Instance.RedNightSky;
                        break;
                }
            }
            StartCoroutine(SchoolEscapeMusic());
        }

        if (exitsReached == 2)
        {
            audioDevice.PlayOneShot(aud_Switch, 0.8f);
            audioDevice.clip = aud_ChaosStart;
            audioDevice.Play();
        }

        if (exitsReached == 3)
        {
            StartCoroutine(audioQueue.FadeOut(escapeMusic, 1f));
            audioDevice.PlayOneShot(aud_Switch, 0.8f);
            audioDevice.clip = aud_ChaosBuildUp;
            audioDevice.Play();
        }
    }

    private IEnumerator exitEasing(int exitCount)
    {
        float duration = 7f;
        Color start = RenderSettings.ambientLight;
        Color target = Color.Lerp(preFinalColor, finalColor, Mathf.Clamp01(exitCount / 3f));

        for (float t = 0; t < 1; t += Time.deltaTime / duration)
        {
            RenderSettings.ambientLight = Color.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        RenderSettings.ambientLight = target;
    }
    #endregion

    #region PlayerTeleportation
    public void CraftersTeleport()
    {
        if (player.hugging)
        {
            player.hugging = false;
            player.sweepingFailsave = 0f;
        }
        else if (player.jumpRope)
        {
            player.jumpRope = false;
            player.DeactivateJumpRope();
            player.playtime.Disappoint();
        }

        var newPos = AILocationSelector.SetNewTargetForAgent(null, AILocationSelectorScript.LocationType.Default) + Vector3.up * player.height;
        player.transform.position = newPos;
        baldi.transform.position = newPos;
    }

    public IEnumerator TeleporterFunction()
    {
        player.movementLocked = true;
        playerCollider.enabled = false;

        int teleports = Random.Range(12, 16);
        float delay = 0.2f;
        const float increaseFactor = 1.1f;

        for (int i = 0; i < teleports; i++)
        {
            yield return new WaitForSeconds(delay);
            PlayerTeleport();
            delay *= increaseFactor;
        }

        player.movementLocked = false;
        playerCollider.enabled = true;
    }

    private void PlayerTeleport()
    {
        player.transform.position = AILocationSelector.SetNewTargetForAgent(null, AILocationSelectorScript.LocationType.Default) + Vector3.up * player.height;
        audioDevice.PlayOneShot(aud_Teleport);
    }
    #endregion

    #region SerializedFields
    [Header("Player & Camera References")]
    public PlayerScript player;
    public Transform cameraTransform;
    public Camera PlayerCamera;
    public Collider playerCollider;
    public CharacterController playerCharacter;

    [Header("Scripts")]
    public KeyFunctions KF;
    public BaldiScript baldiScrpt;
    public CraftersScript sockCript;
    public PlaytimeScript playtimeScript;
    public FirstPrizeScript firstPrizeScript;
    [SerializeField] private AILocationSelectorScript AILocationSelector;

    [Header("Game Mode & Settings")]
    public string mode = "story";
    public int notebooks = 0, maxNotebooks = 7, UnlockAmount = 2;
    public bool debugMode;
    [SerializeField] private string gameoverScene = "GameOver";

    [Header("Serialized References")]
    [SerializeField] private TMP_Text notebookCount;
    [SerializeField] private List<EntranceScript> entrances = new List<EntranceScript>();
    [SerializeField] private GameObject highScoreText, baldi;
    public List<GameObject> ObjectsToEnable = new List<GameObject>();
    [SerializeField] private List<GameObject> ObjectsToDisable, ItemsToRespawn = new List<GameObject>();
    [SerializeField] private List<VendingMachineScript> MachinesToRestock = new List<VendingMachineScript>();
    public Animator Icon;
    public Material SpriteRenderer;
    public Sprite Present;
    public GameObject learningGame;
    [SerializeField] private Color preFinalColor = new Color(1, 0.7f, 0.7f), finalColor = Color.red;

    [Header("Audio References")]
    [SerializeField] private AudioClip[] LoseSounds;
    public AudioSource audioDevice, schoolMusic, escapeMusic, gamaOvarDevice;
    public AudioClip aud_Hang, aud_Rattling, aud_Unlocked, aud_ItemCollect, SchoolhouseEscape, Slowed_SchoolhouseEscape, aud_Collected;
    [SerializeField] private AudioClip aud_ChaosStart, aud_ChaosStartLoop, aud_ChaosBuildUp, aud_ChaosFinal, aud_Switch, aud_Teleport;
    #endregion

    #region PrivateFields
    private AudioQueueScript audioQueue;
    private int audOverVal;
    private float gameOverDelay = 0.5f;
    [HideInInspector] public int lastRespawnCount, failedNotebooks, exitsReached, cullingMask;
    [HideInInspector] public bool spoopMode = false, finaleMode = false;
    [HideInInspector] public Coroutine exitEasingCoroutine;
    [HideInInspector] public LearningGameManager Math;
    [HideInInspector] public EndingManager progress;
    [HideInInspector] public static bool ChaosMode;
    #endregion
}