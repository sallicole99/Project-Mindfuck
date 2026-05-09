using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UISplashScreenManager : MonoBehaviour
{
    #region Unity Lifecycle
    private void Awake()
    {
        Sych.SetCursorLock(true);

        if (splashImage == null)
        {
            Debug.LogError("Assign Splash Image in inspector!");
        }

        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        SetImageAlpha(0f);
        splashImage.gameObject.SetActive(true);
    }

    private void Start()
    {
        Singleton<Options>.Instance.GetVolume();
        Singleton<Options>.Instance.GetVSync();

        if (skipSplash)
        {
            splashImage.gameObject.SetActive(false);
            ShowWarning();
            return;
        }

        StartCoroutine(PlaySplashScreens());
    }
    #endregion

    #region Splash Logic
    private IEnumerator PlaySplashScreens()
    {
        float totalDuration = 0f;
        foreach (var splash in splashScreens)
        {
            totalDuration += splash.displayDuration + splash.fadeDuration;
        }

        foreach (var splash in splashScreens)
        {
            if (splash.logoSprite == null) continue;

            splashImage.sprite = splash.logoSprite;
            SetImageAlpha(1f);

            if (splash.soundEffect != null)
            {
                audioSource.clip = splash.soundEffect;
                audioSource.Play();
            }

            yield return new WaitForSeconds(splash.displayDuration);
            yield return StartCoroutine(FadeImageAlpha(1f, 0f, splash.fadeDuration));
        }

        splashImage.gameObject.SetActive(false);
        ShowWarning();
    }
    #endregion

    #region UI Behavior
    private IEnumerator FadeImageAlpha(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = splashImage.color;
        color.a = fromAlpha;
        splashImage.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            splashImage.color = color;
            yield return null;
        }

        color.a = toAlpha;
        splashImage.color = color;
    }

    private void SetImageAlpha(float alpha)
    {
        Color color = splashImage.color;
        color.a = alpha;
        splashImage.color = color;
    }

    private void ShowWarning()
    {
        Sych.SetCursorLock(false);
        warningPanel.SetActive(true);
    }
    #endregion

    #region Structs
    [System.Serializable]
    public struct SplashScreen
    {
        public Sprite logoSprite;
        public float fadeDuration;
        public float displayDuration;
        public AudioClip soundEffect;
    }
    #endregion

    #region Inspector Fields
    [Header("UI References")]
    [SerializeField] private Image splashImage;
    [SerializeField] private GameObject warningPanel;

    [Header("Splash Screens")]
    [SerializeField] private List<SplashScreen> splashScreens;

    [Header("Settings")]
    [SerializeField] private bool skipSplash = false;
    #endregion

    #region Private Fields
    private AudioSource audioSource;
    #endregion
}