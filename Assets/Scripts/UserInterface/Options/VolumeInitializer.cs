using UnityEngine;

public static class VolumeInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void InitializeVolume()
    {
        float savedSliderValue = PlayerPrefs.GetFloat("Volume", 9f);
        float normalizedVolume = Mathf.Clamp01(savedSliderValue / 9f);
        AudioListener.volume = normalizedVolume;
    }
}