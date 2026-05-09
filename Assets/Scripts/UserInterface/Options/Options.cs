using UnityEngine;

public class Options : Singleton<Options>
{
    public void Start()
    {
        float savedSliderValue = PlayerPrefs.GetFloat("Volume", 9f);
        float normalized = Mathf.Clamp01(savedSliderValue / 9f);
        SetVolume(normalized);

        int vSyncSetting = PlayerPrefs.GetInt("VSync", 0);
        SetVSync(vSyncSetting);

        bool isFullscreen = PlayerPrefsExtension.GetBool("Fullscreen", true);
        SetFullscreen(isFullscreen);

        resolutions = Screen.resolutions;
        int resolutionIndex = PlayerPrefs.GetInt("Resolution", GetCurrentResolutionIndex());
        SetResolution(resolutionIndex);

        bool runInBackground = PlayerPrefsExtension.GetBool("RunInBackground", false);
        SetRunInBackground(runInBackground);
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        AudioListener.volume = volume;
    }

    public float GetVolume() => volume;

    public void SetVSync(int value)
    {
        QualitySettings.vSyncCount = value;
        PlayerPrefs.SetInt("VSync", value);
        PlayerPrefs.Save();
    }

    public int GetVSync() => PlayerPrefs.GetInt("VSync", 0);

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefsExtension.SetBool("Fullscreen", isFullscreen);
    }

    public void SetResolution(int index)
    {
        if (resolutions == null || resolutions.Length == 0)
        {
            resolutions = Screen.resolutions;
        }

        if (index >= 0 && index < resolutions.Length)
        {
            Resolution res = resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt("Resolution", index);
            PlayerPrefs.Save();
        }
    }

    private int GetCurrentResolutionIndex()
    {
        resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }
        return 0;
    }

    public void SetRunInBackground(bool runInBackground)
    {
        Application.runInBackground = runInBackground;
        PlayerPrefsExtension.SetBool("RunInBackground", runInBackground);
    }

    private float volume = 1f;
    private Resolution[] resolutions;
}