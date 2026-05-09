using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Gauge : CircleSlider
{
    private void Update()
    {
        if (!shows)
        {
            time -= Time.deltaTime;
        }
        else
        {
            value = Mathf.Max(value - 1f * Time.deltaTime, Mathf.Min(value + 1f * Time.deltaTime, specialValue));
            base.UpdateSliderValue(value);
        }
    }

    public void Show(Sprite iconSprite, float showTime)
    {
        shows = true;
        gaugeAnimator.Play(animationNames[0]);
        specialValue = Mathf.Clamp(showTime / showTime, 0f, 1f);
        time = showTime;
        icon.sprite = iconSprite;
    }

    public void Hide()
    {
        shows = false;
        gaugeAnimator.Play(animationNames[1]);
        IEnumerator Animation()
        {
            yield return new WaitForSeconds(1.2f);
            GaugeManager.Instance.gauges.Remove(this);
            Destroy(gameObject);
        }
        StartCoroutine(Animation());
    }

    public void Set(float total, float remain)
    {
        specialValue = Mathf.Clamp(remain / total, 0f, 1f);
        time = remain;
    }

    private float specialValue;
    private float value;
    [HideInInspector] public float time;
    private bool shows;

    public bool Activated
    {
        get
        {
            return shows;
        }
    }

    [Header("Icon"), SerializeField, Tooltip("The Icon image")] private Image icon;

    [Header("Animation"), SerializeField, Tooltip("Animation of the gauge")] private Animator gaugeAnimator;

    [SerializeField, Tooltip("Animation Names")] private string[] animationNames = { "GaugeActivate", "GaugeDeactivate" };
}
