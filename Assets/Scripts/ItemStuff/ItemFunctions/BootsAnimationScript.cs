using UnityEngine;
using System.Collections;

public class BootsAnimationScript : MonoBehaviour
{
    #region Initialization
    private void Start()
    {
        Lastings = 60f;
        StartCoroutine(BootAnimation());
        maxGaugeLimit = Lastings;

        if (gauge == null)
        {
            gauge = GaugeManager.Instance.CreateGaugeInstance(gaugeSprite, maxGaugeLimit);
        }
    }
    #endregion

    #region Per-Frame Logic
    private void Update()
    {
        if (Lastings > 0f)
        {
            Lastings -= Time.deltaTime;
        }

        if (gauge != null)
        {
            if (Lastings > 0f)
            {
                gauge.Set(maxGaugeLimit, Lastings);
            }
            else
            {
                gauge.Hide();
            }
        }
    }
    #endregion

    #region Boot Animation
    private IEnumerator BootAnimation()
    {
        bootsAnimator.Play("Boots_Down");
        yield return new WaitForSeconds(Lastings);

        bootsAnimator.Play("Boots_Up");

        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo stateInfo = bootsAnimator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName("Boots_Up") && stateInfo.normalizedTime >= 1.0f;
        });

        Destroy(gameObject);
    }
    #endregion

    #region Serialized Configuration
    [SerializeField] private Animator bootsAnimator;
    [SerializeField] private Sprite gaugeSprite;
    [SerializeField] private float Lastings;
    #endregion

    #region Internal State
    private float maxGaugeLimit;
    private Gauge gauge;
    #endregion
}