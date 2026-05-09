using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum OptionType
{
    Toggle,
    Slider,
    Dropdown
}

public class OptionIndex : MonoBehaviour
{
    private void Start()
    {
        if (GetComponent<Slider>() != null)
        {
            OptionType = OptionType.Slider;
        }
        else if (GetComponent<Toggle>() != null)
        {
            OptionType = OptionType.Toggle;
        }
        else if (GetComponent<TMP_Dropdown>() != null)
        {
            OptionType = OptionType.Dropdown;
        }

        switch (OptionType)
        {
            case OptionType.Toggle:
                Toggle toggle = GetComponent<Toggle>();
                toggle.isOn = PlayerPrefsExtension.GetBool(OptionName);
                toggle.onValueChanged.AddListener(ChangeOption);
                break;

            case OptionType.Slider:
                Slider slider = GetComponent<Slider>();
                if (PlayerPrefs.GetFloat(OptionName, -999) == -999)
                {
                    PlayerPrefs.SetFloat(OptionName, slider.value);
                }
                slider.value = PlayerPrefs.GetFloat(OptionName);
                slider.onValueChanged.AddListener(ChangeOption);
                break;

            case OptionType.Dropdown:
                TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();
                int savedValue = PlayerPrefs.GetInt(OptionName, dropdown.value);
                dropdown.value = savedValue;
                dropdown.RefreshShownValue();
                dropdown.onValueChanged.AddListener(ChangeOption);
                break;
        }
    }

    public void ChangeOption(bool value)
    {
        PlayerPrefsExtension.SetBool(OptionName, value);

        if (OptionName == "VSync")
        {
            Singleton<Options>.Instance.SetVSync(value ? 1 : 0);
        }
        else if (OptionName == "Fullscreen")
        {
            Singleton<Options>.Instance.SetFullscreen(value);
        }
        else if (OptionName == "RunInBackground")
        {
            Singleton<Options>.Instance.SetRunInBackground(value);
        }
    }

    public void ChangeOption(float value)
    {
        if (OptionName == "Volume")
        {
            float normalized = Mathf.Clamp01(value / 9f);
            Singleton<Options>.Instance.SetVolume(normalized);
            PlayerPrefs.SetFloat(OptionName, value);
            PlayerPrefs.Save();
        }
        else
        {
            PlayerPrefs.SetFloat(OptionName, value);
            PlayerPrefs.Save();
        }
    }

    public void ChangeOption(int index)
    {
        if (OptionName == "Resolution")
        {
            Singleton<Options>.Instance.SetResolution(index);
        }

        PlayerPrefs.SetInt(OptionName, index);
        PlayerPrefs.Save();
    }

    [SerializeField] private string OptionName;
    private OptionType OptionType;
}