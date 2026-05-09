using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[ExecuteAlways]
#else
[ExecuteInEditMode]
#endif

public class SliderKnob : MonoBehaviour
{
    private void Update() => CircleChange(slider.value);
    
    private void CircleChange(float sliderValue)
    {
        float invertedSliderValue = 1f - sliderValue / slider.maxValue;
        float ratio = sliderValue / slider.maxValue;
        float angle = ratio + invertedSliderValue * 360f;
        float radians = angle * Mathf.Deg2Rad;

        float x = Mathf.Cos(radians) * circleRadius;
        float y = Mathf.Sin(radians) * circleRadius;

        knob.localPosition = new Vector3(x, y, 0f);

        float rotationAngle = angle - 90f;
        knob.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
    }

    [SerializeField] private RectTransform knob;
    [SerializeField] private Slider slider;
    [SerializeField] private float circleRadius = 42f;
}
