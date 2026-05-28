using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPSystem : MonoBehaviour
{
    // ─── HP Settings ──────────────────────────────────────────────────
    [Header("HP Settings")]
    public float maxHP              = 175f;
    public float baldiDamage        = 10f;
    public float outdoorRegenDelay  = 25f;
    public float outdoorRegenAmount = 1f;

    [Header("Damage Cooldown")]
    public float damageCooldown  = 1.5f;
    private float lastDamageTime = -999f;

    // ─── UI ───────────────────────────────────────────────────────────
    [Header("HP Bar UI")]
    public Slider hpBar;          // main red HP bar — on top
    public Slider ghostBar;       // white delayed bar — sits behind red bar
    public Image  backgroundImage;// solid background behind everything
    public Image  hpFillImage;    // fill image of main red bar
    public TMP_Text hpText;
    public Color healthyColor    = Color.red;
    public Color lowColor        = new Color(0.8f, 0f, 0f);
    public Color ghostBarColor   = Color.white;
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f); // dark grey background

    // ─── Internal ─────────────────────────────────────────────────────
    private float currentHP;
    private float ghostHP;
    private bool  ghostWaiting = false;
    private bool  isOutdoors   = false;
    private float outdoorTimer = 0f;
    private bool  isDead       = false;

    private PlayerScript player;
    private GameControllerScript gc;

    // ─────────────────────────────────────────────────────────────────
    void Start()
    {
        player    = GetComponent<PlayerScript>();
        gc        = FindObjectOfType<GameControllerScript>();
        currentHP = maxHP;
        ghostHP   = maxHP;

        // Set ghost bar color
        if (ghostBar != null && ghostBar.fillRect != null)
        {
            var ghostFill = ghostBar.fillRect.GetComponent<Image>();
            if (ghostFill != null) ghostFill.color = ghostBarColor;
        }

        // Set background color
        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;
    }

    void Update()
    {
        if (isDead) return;
        HandleOutdoorRegen();
        UpdateUI();
    }

    // ─── Outdoor Regen ────────────────────────────────────────────────
    private void HandleOutdoorRegen()
    {
        if (!isOutdoors) return;

        outdoorTimer += Time.deltaTime;
        if (outdoorTimer >= outdoorRegenDelay)
        {
            outdoorTimer = 0f;
            Heal(outdoorRegenAmount);
        }
    }

    // ─── UI ───────────────────────────────────────────────────────────
    private void UpdateUI()
    {
        // Red bar drops fast
        if (hpBar != null)
            hpBar.value = Mathf.Lerp(hpBar.value, currentHP / maxHP, Time.deltaTime * 10f);

        // Ghost bar holds still during wait, then drains slowly
        if (ghostBar != null && !ghostWaiting)
            ghostBar.value = Mathf.Lerp(ghostBar.value, currentHP / maxHP, Time.deltaTime * 2f);

        if (hpFillImage != null)
            hpFillImage.color = currentHP / maxHP < 0.3f ? lowColor : healthyColor;

        if (hpText != null)
            hpText.text = Mathf.CeilToInt(currentHP) + " / " + Mathf.CeilToInt(maxHP);
    }

    // ─── Take Damage ───
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        ghostHP   = currentHP; // snapshot HP before damage so ghost holds here
        currentHP -= amount;
        currentHP  = Mathf.Max(currentHP, 0f);

        StopAllCoroutines();
        if (amount >= 50f)
            StartCoroutine(GhostBarDelay(2f));
        else
            StartCoroutine(GhostBarDelay(1f));

        if (currentHP <= 0f)
            Die();
    }

    private IEnumerator GhostBarDelay(float waitTime)
    {
        ghostWaiting = true;

        // Snap ghost bar to pre-damage position and hold it
        if (ghostBar != null)
            ghostBar.value = ghostHP / maxHP;

        yield return new WaitForSeconds(waitTime);

        // Release — UpdateUI lerps it down to match red bar
        ghostWaiting = false;
    }

    // ─── Heal ───
    public void Heal(float amount)
    {
        if (isDead) return;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    // ─── Death ───
    private void Die()
    {
        isDead = true;
        player.TriggerBaldiGameOver();
    }

    // ─── Outdoor Zone Regen ──
    public void EnterOutdoors()
    {
        isOutdoors   = true;
        outdoorTimer = 0f;
    }

    public void ExitOutdoors()
    {
        isOutdoors   = false;
        outdoorTimer = 0f;
    }

    // ─── Baldi Damage ───
    public void TakeBaldiDamage()
    {
        if (Time.time - lastDamageTime < damageCooldown) return;
        lastDamageTime = Time.time;
        TakeDamage(baldiDamage);
        FindObjectOfType<CameraScript>()?.TriggerDamageTilt();
    }
}