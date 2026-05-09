using UnityEngine;
using UnityEngine.UI;

public class CircleSlider : MonoBehaviour
{
    public virtual void UpdateSliderValue(float value) => sliderImage.sprite = sliderSprites[Mathf.RoundToInt((1f - Mathf.Clamp(value, 0f, 1f)) * (sliderSprites.Length - 1))];

    public bool GetSliderSprite(int id)
    {
        if (sliderImage.sprite == sliderSprites[id])
        {
            return true;
        }
        return false;
    }

    [Header("Circle Slider")]
    public Sprite[] sliderSprites;
    public Image sliderImage;
}
