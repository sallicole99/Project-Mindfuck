using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsMenu : MonoBehaviour
{
    #region Lifecycle
    private void OnEnable()
    {
        showTime = 0f;
        UpdateAllTexts();
    }

    private void Update() => HandleRebindCountdown();
    #endregion

    #region Key Rebinding Logic
    private void HandleRebindCountdown()
    {
        if (showTime > 0f)
        {
            counterText.text = string.Format("Press a key to assign it to {0} in {1} seconds.", labelTexts[currentClick].text, Mathf.CeilToInt(showTime).ToString());

            if (Input.GetMouseButtonDown(0))
            {
                ApplyNewBinding(KeyCode.Mouse0);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                ApplyNewBinding(KeyCode.Mouse1);
            }

            foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode) && !kcode.ToString().Contains("Mouse"))
                {
                    ApplyNewBinding(kcode);
                }
            }

            showTime -= Time.unscaledDeltaTime;
        }
        else if (panel.activeSelf)
        {
            panel.SetActive(false);
            foreach (Button but in FindObjectsOfType<Button>())
                but.interactable = true;
        }
    }

    private void ApplyNewBinding(KeyCode key)
    {
        showTime = 0f;
        Singleton<InputManager>.Instance.Modifiy((InputAction)currentClick, key);
        Singleton<InputManager>.Instance.Save("DoNotShip");
        Debug.Log(labelTexts[currentClick].text + " setting has been changed to " + key);
        UpdateAllTexts();
    }
    #endregion

    #region UI Update Logic
    public void UpdateAllTexts()
    {
        for (int i = 0; i < buttonTexts.Length; i++)
        {
            KeyCode key = Singleton<InputManager>.Instance.KeyboardMapping[(InputAction)i];
            buttonTexts[i].text = key.ToString();
        }
    }
    #endregion

    #region Confirmation Menu Handling
    public void ShowScreen(int num)
    {
        currentClick = num;
        changeKeyConfirmationPanel.SetActive(true);
        foreach (Button but in FindObjectsOfType<Button>())
        {
            if (but.transform.parent != changeKeyConfirmationPanel.transform)
                but.interactable = false;
        }
    }

    public void ShowResetConfirmation()
    {
        resetConfirmationPanel.SetActive(true);
        foreach (Button but in FindObjectsOfType<Button>())
        {
            if (but.transform.parent != resetConfirmationPanel.transform)
                but.interactable = false;
        }
    }

    public void ConfirmRebind()
    {
        changeKeyConfirmationPanel.SetActive(false);
        panel.SetActive(true);
        showTime = 5f;
    }

    public void ConfirmReset()
    {
        resetConfirmationPanel.SetActive(false);
        SetDefaults();
        foreach (Button but in FindObjectsOfType<Button>())
            but.interactable = true;
    }

    public void CancelRebind()
    {
        changeKeyConfirmationPanel.SetActive(false);
        foreach (Button but in FindObjectsOfType<Button>())
            but.interactable = true;
    }

    public void CancelReset()
    {
        resetConfirmationPanel.SetActive(false);
        foreach (Button but in FindObjectsOfType<Button>())
            but.interactable = true;
    }
    #endregion

    #region Defaults & Reset
    public void SetDefaults()
    {
        Singleton<InputManager>.Instance.SetDefaults();
        Singleton<InputManager>.Instance.Save("DoNotShip");
        UpdateAllTexts();
    }
    #endregion

    #region Serialized Fields
    [Header("Texts References")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text[] buttonTexts, labelTexts;

    [Header("UI Panels")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject changeKeyConfirmationPanel, resetConfirmationPanel;

    [Header("Settings")]
    [SerializeField] private float showTime;
    [SerializeField] private int currentClick;
    #endregion
}