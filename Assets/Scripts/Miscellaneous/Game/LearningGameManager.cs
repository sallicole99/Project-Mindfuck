using UnityEngine;

public class LearningGameManager : MonoBehaviour
{
    #region UnityCallbacks
    private void Start()
    {
        Emag = GetComponent<GameControllerScript>();
        audioQueue = GetComponent<AudioQueueScript>();
    }
    #endregion

    #region LearningStateManagement
    public void ActivateLearningGame()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        learningActive = true;
        KF.UnlockMouse();
        Tutor.tutorSource.Stop();

        if (!Emag.spoopMode)
        {
            StartCoroutine(audioQueue.FadeOut(Emag.schoolMusic, 0.25f));
            learnMusic.Play();
        }
    }

    public void DeactivateLearningGame(GameObject subject)
    {
        Emag.schoolMusic.ignoreListenerPause = false;
        AudioListener.pause = false;
        Time.timeScale = 1f;
        Emag.PlayerCamera.cullingMask = Emag.cullingMask;
        learningActive = false;
        Destroy(subject);
        KF.LockMouse();
        Emag.audioDevice.PlayOneShot(Emag.aud_Collected);
        Emag.Icon.Rebind();
        Emag.Icon.Play("IconSpin", -1, 0f);

        if (AdditionalGameCustomizer.Instance != null && AdditionalGameCustomizer.Instance.ExitCounter && Emag.notebooks == Emag.maxNotebooks && Emag.mode == "story")
        {
            Emag.Icon.Rebind();
            Emag.Icon.Play("Icon2Idle", -1, 0f);
        }

        if (Emag.player.stamina < 100f)
        {
            Emag.player.SetStamina(PlayerScript.StaminaChangeMode.Set, 100f);
        }

        if (!Emag.spoopMode)
        {
            Emag.schoolMusic.Play();
            StartCoroutine(audioQueue.FadeOut(learnMusic, 0.25f));
        }

        if (Emag.notebooks == 1 && !Emag.spoopMode)
        {
            quarter.SetActive(true);
            Tutor.tutorSource.PlayOneShot(aud_Prize);
        }
        else if (Emag.notebooks == Emag.maxNotebooks && Emag.mode == "story")
        {
            if (AdditionalGameCustomizer.Instance != null &&
                AdditionalGameCustomizer.Instance.FinalModeTV)
            {
                StartCoroutine(Television.StartTVSequence(aud_AllNotebooks));
            }
            else
            {
                Emag.audioDevice.PlayOneShot(aud_AllNotebooks, 0.8f);
            }

            Emag.escapeMusic.clip = Emag.SchoolhouseEscape;
            Emag.escapeMusic.loop = true;
            Emag.escapeMusic.Play();
        }
    }
    #endregion

    #region References
    [Header("References")]
    public AudioSource learnMusic;
    public AudioClip aud_AllNotebooks, aud_Prize;
    public GameObject quarter;

    [Header("Scripts")]
    [SerializeField] private KeyFunctions KF;
    public FinalModeTV Television;
    public TutorScript Tutor;
    #endregion

    #region PrivateStates
    private GameControllerScript Emag;
    private AudioQueueScript audioQueue;
    [HideInInspector] public bool learningActive;
    #endregion
}