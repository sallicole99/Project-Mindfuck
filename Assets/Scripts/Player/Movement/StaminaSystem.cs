using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaSystem : MonoBehaviour
{
    // ─── Stamina Settings ─────────────────────────────────────────────
    [Header("Stamina Settings")]
    public float maxStamina         = 150f;  // normal maximum
    public float outdoorMaxStamina  = 500f;  // maximum while outdoors
    public float staminaDrop        = 10f;   // drain rate while running (Shift)
    public float sprintDrop         = 35f;   // drain rate while sprinting (Shift+Tab)
    public float staminaRise        = 20f;   // regen rate while not running
    public float outdoorRise        = 15f;   // extra regen while outdoors
    public float negativeFaintLimit = -50f;  // faint at this value

    // ─── UI ───────────────────────────────────────────────────────────
    [Header("Stamina Bar UI")]
    public Slider staminaBar;
    public Image  staminaFillImage;
    public TMP_Text staminaPercentText;
    public Color  normalColor      = Color.green;
    public Color  runColor         = Color.yellow;
    public Color  sprintColor      = Color.red;
    public Color  negativeColor    = new Color(0.5f, 0f, 0f);
    public Color  outdoorFullColor = Color.cyan;

    [Header("Outdoor Full Indicator")]
    public GameObject outdoorFullIndicator;

    // ─── Internal ─────────────────────────────────────────────────────
    private float stamina;
    private bool  isOutdoors = false;
    private bool  hasFainted = false;

    private PlayerScript        player;
    private CharacterController cc;

    // ─────────────────────────────────────────────────────────────────
    void Start()
    {
        player  = GetComponent<PlayerScript>();
        cc      = GetComponent<CharacterController>();
        stamina = maxStamina;
        player.stamina = stamina;

        if (outdoorFullIndicator != null)
            outdoorFullIndicator.SetActive(false);
    }

    void FixedUpdate()
    {
        if (player.movementLocked || hasFainted) return;
        HandleStamina();
    }

    void Update()
    {
        if (hasFainted) return;
        UpdateUI();
    }

    // ─── Phase Detection ──────────────────────────────────────────────
    private bool IsSprinting =>
        Singleton<InputManager>.Instance.GetActionKey(InputAction.Run) &&
        Input.GetKey(KeyCode.Tab);

    private bool IsRunning =>
        Singleton<InputManager>.Instance.GetActionKey(InputAction.Run) &&
        !Input.GetKey(KeyCode.Tab);

    // ─── Stamina Logic ────────────────────────────────────────────────
    private void HandleStamina()
    {
        bool isMoving    = cc.velocity.magnitude > 0.1f;
        float currentMax = isOutdoors ? outdoorMaxStamina : maxStamina;

        if (IsSprinting && isMoving)
        {
            stamina -= sprintDrop * Time.fixedDeltaTime;
        }
        else if (IsRunning && isMoving)
        {
            stamina -= staminaDrop * Time.fixedDeltaTime;
        }
        else
        {
            float regen = staminaRise;
            if (isOutdoors) regen += outdoorRise;

            // Indoors: only regen when standing still
            // Outdoors: regen while walking too
            bool canRegen = isOutdoors ? true : cc.velocity.magnitude < 0.1f;

            if (canRegen && stamina < currentMax)
            {
                stamina += regen * Time.fixedDeltaTime;
                stamina = Mathf.Min(stamina, currentMax); // only clamp during active regen
            }
            // No clamp outside regen — stamina above 150 persists after leaving outdoors
        }

        // Sync to PlayerScript
        player.stamina = stamina;

        // Faint check
        if (stamina <= negativeFaintLimit && !hasFainted)
            StartCoroutine(FaintSequence());
    }

    // ─── UI ───────────────────────────────────────────────────────────
    private void UpdateUI()
    {
        if (staminaBar == null) return;

        // Always treat maxStamina (150) as full bar — going above just keeps it full
        float range      = maxStamina - negativeFaintLimit;
        float normalized = (stamina - negativeFaintLimit) / range;
        staminaBar.value = Mathf.Lerp(staminaBar.value, Mathf.Clamp01(normalized), Time.deltaTime * 8f);

        if (staminaFillImage != null)
        {
            if (stamina < 0f)
                staminaFillImage.color = negativeColor;
            else if (isOutdoors && stamina >= outdoorMaxStamina)
                staminaFillImage.color = outdoorFullColor;
            else if (IsSprinting)
                staminaFillImage.color = sprintColor;
            else if (IsRunning)
                staminaFillImage.color = runColor;
            else
                staminaFillImage.color = normalColor;
        }

        if (outdoorFullIndicator != null)
            outdoorFullIndicator.SetActive(isOutdoors && stamina >= outdoorMaxStamina);

        if (staminaPercentText != null)
            staminaPercentText.text = Mathf.RoundToInt(stamina) + "%";
    }

    // ─── Faint ────────────────────────────────────────────────────────
    private IEnumerator FaintSequence()
    {
        hasFainted = true;
        player.movementLocked = true;
        stamina = 0f;
        player.stamina = 0f;
        player.TriggerBaldiGameOver();
        yield return new WaitForSeconds(0.3f);
    }

    // ─── Outdoor Zone API ─────────────────────────────────────────────
    public void EnterOutdoors()
    {
        isOutdoors = true;
    }

    public void ExitOutdoors()
    {
        isOutdoors = false;
        // Stamina stays wherever it is — no reset
    }

    // ─── Public Setter ────────────────────────────────────────────────
    public void SetStamina(float value)
    {
        float currentMax = isOutdoors ? outdoorMaxStamina : maxStamina;
        stamina = Mathf.Clamp(value, negativeFaintLimit, currentMax);
        player.stamina = stamina;
    }
}