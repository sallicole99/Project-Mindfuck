using UnityEngine;

public class AlarmClockScript : MonoBehaviour
{
    #region Initialization
    private void Start()
    {
        ClockRender = GetComponentInChildren<SpriteRenderer>();

        currentPreset = System.Array.IndexOf(timePresets, 30);
        if (currentPreset < 0) currentPreset = 0;
        timeLeft = timePresets[currentPreset];
        lifeSpan = timeLeft + 5;
    }
    #endregion

    #region Per-Frame Logic
    private void Update()
    {
        if (timeLeft.CountdownWithDeltaTime() != 0)
        {
            UpdateDisplay();
        }
        else if (!rang)
        {
            TriggerAlarm();
        }

        if (lifeSpan.CountdownWithDeltaTime() == 0)
        {
            Destroy(gameObject);
        }

        if (Time.timeScale != 0 && trigger.ScreenRaycastMatchesCollider(out _, 15) && !rang && (Input.GetMouseButtonDown(0) || Singleton<InputManager>.Instance.GetActionKey(InputAction.Interact)))
        {
            CyclePreset();
        }
    }
    #endregion

    #region Alarm Control
    private void CyclePreset()
    {
        currentPreset = (currentPreset + 1) % timePresets.Length;
        timeLeft = timePresets[currentPreset];
        lifeSpan = timePresets[currentPreset] + 5;
        windAud.Play();
    }

    private void TriggerAlarm()
    {
        rang = true;
        gameObject.tag = "Untagged";
        baldi.Hear(transform.position, 8);
        audioDevice.PlayClip(ring, false, 1f);
    }

    private void UpdateDisplay()
    {
        var index = Mathf.Clamp((int)(timeLeft / 15), 0, 3);
        currentPreset = index;
        ClockRender.sprite = sprites[index];
    }
    #endregion

    #region Serialized Configuration
    [Header("References")]
    public BaldiScript baldi;

    [Header("Audio")]
    [SerializeField] private AudioClip ring;
    [SerializeField] private AudioSource audioDevice, windAud;

    [Header("Settings")]
    [SerializeField] private int[] timePresets = { 15, 30, 45, 60 };
    [SerializeField] private float timeLeft, lifeSpan;

    [Header("Visuals")]
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private SphereCollider trigger;
    #endregion

    #region Internal State
    private SpriteRenderer ClockRender;
    [HideInInspector] public bool rang;
    [HideInInspector] public int currentPreset;
    #endregion
}