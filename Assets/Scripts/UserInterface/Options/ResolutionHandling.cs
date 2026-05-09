using TMPro;
using UnityEngine;
using System.Linq;

public class ResolutionHandling : MonoBehaviour
{
    private void Start()
    {
        var resolutions = Screen.resolutions;
        var options = resolutions.Select(r => $"{r.width} × {r.height}").ToList();

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);

        int currentIndex = System.Array.FindIndex(resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);

        resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", currentIndex);
        resolutionDropdown.RefreshShownValue();
    }

    [SerializeField] private TMP_Dropdown resolutionDropdown;
}