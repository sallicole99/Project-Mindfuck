using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextUnderliner : MonoBehaviour
{
    public void Underline(bool enableUnderline) => text.fontStyle = (text.fontStyle & ~FontStyles.Underline) | (enableUnderline ? FontStyles.Underline : 0);
    
    [SerializeField] private TMP_Text text;
}