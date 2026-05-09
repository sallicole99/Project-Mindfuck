using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WarningScreenScript : MonoBehaviour
{
    private void Start()
    {
        if (sprites.Length > 0 && displayImage != null)
        {
            displayImage.sprite = sprites[0];
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            Advance();
        }
    }

    private void Advance()
    {
        current++;
        if (current < sprites.Length)
        {
            displayImage.sprite = sprites[current];
        }
        else
        {
            SceneManager.LoadScene(BootUp);
        }
    }

	[Header("Scene Settings")]
    [SerializeField] private string BootUp;

    [Header("UI References")]
    [SerializeField] private Image displayImage;
    [SerializeField] private Sprite[] sprites;

    private int current = 0;
}
