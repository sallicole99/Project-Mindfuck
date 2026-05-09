using TMPro;
using UnityEngine;

public class DetentionTextScript : MonoBehaviour
{
    private void Awake() => spriteRenderers = new SpriteRenderer[] {minuteTensRenderer, minuteOnesRenderer, ColonRenderer, secondTensRenderer, secondOnesRenderer};

    private void Update()
    {
        bool useNewTimer = AdditionalGameCustomizer.Instance?.OldDetentionTimer == false;
        bool hasTime = door.lockTime > 0f;

        if (useNewTimer)
        {
            float displayTime = hasTime ? door.lockTime : 0f;
            UpdateDisplay(displayTime, hasTime ? numberColor : numberOutColor);
        }
        else
        {
            TimerText.text = hasTime ? DetentionText + "\n" + Mathf.CeilToInt(door.lockTime) + SecondsText : string.Empty;
        }
    }

    private void UpdateDisplay(float time, Color color)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        UpdateDigitDisplay(minutes / 10, minuteTensRenderer);
        UpdateDigitDisplay(minutes % 10, minuteOnesRenderer);
        UpdateDigitDisplay(seconds / 10, secondTensRenderer);
        UpdateDigitDisplay(seconds % 10, secondOnesRenderer);

        SetSpriteColors(color);
    }

    private void UpdateDigitDisplay(int digit, SpriteRenderer renderer)
    {
        if (digit >= 0 && digit < numberSprites.Length)
        {
            renderer.sprite = numberSprites[digit];
        }
    }

    private void SetSpriteColors(Color color)
    {
        foreach (var renderer in spriteRenderers)
        {
            renderer.color = color;
        }
    }

    [Header("References")]
    [SerializeField] private DoorScript door;
    [SerializeField] private SpriteRenderer minuteTensRenderer, minuteOnesRenderer, ColonRenderer, secondTensRenderer, secondOnesRenderer;

    [Header("Appearance Settings")]
    [SerializeField] private Sprite[] numberSprites;
    [SerializeField] private Color numberColor, numberOutColor;

    [Header("Text Appearance")]
    [SerializeField] private TMP_Text TimerText;
    [SerializeField] private string DetentionText = "You have detention!", SecondsText = "seconds remain!";

    private SpriteRenderer[] spriteRenderers;

}