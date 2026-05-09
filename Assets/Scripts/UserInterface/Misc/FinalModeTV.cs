using UnityEngine;
using System.Collections;

public class FinalModeTV : MonoBehaviour
{
    public IEnumerator StartTVSequence(AudioClip baldiClip)
    {
        yield return StartCoroutine(StartLoweringTV());

        yield return StartCoroutine(PlayBaldiClip(baldiClip));

        yield return StartCoroutine(StartLiftingTV());
    }

    public IEnumerator StartLoweringTV()
    {
        bool showMarkings = Markings;
        float delayTimer = showMarkings ? 3f : 0.75f;

        if (showMarkings)
        {
            WarningMarks.SetActive(true);
            TelevisionDevice.PlayOneShot(markingSound == MarkingSoundType.Alert ? mus_Alert : aud_TimesOutBell);
        }

        BaldiTVAnimator.Rebind();
        BaldiTVAnimator.Play("TV_LowerDown", -1, 0f);

        yield return new WaitForSeconds(delayTimer);

        if (showMarkings) WarningMarks.SetActive(false);

        Static.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        Static.SetActive(false);
        Baldi.SetActive(true);
    }

    public IEnumerator PlayBaldiClip(AudioClip clip)
    {
        if (clip == null) yield break;

        audioQueue.QueueAudio(clip);

        float waitTime = clip.length;
        while (waitTime > 0)
        {
            if (!AudioListener.pause) waitTime -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public IEnumerator StartLiftingTV()
    {
        Baldi.SetActive(false);
        Static.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        Static.SetActive(false);

        BaldiTVAnimator.Rebind();
        BaldiTVAnimator.Play("TV_RiseUp", -1, 0f);
    }

    [Header("Serialized References")]
    [SerializeField] private Animator BaldiTVAnimator;
    [SerializeField] private GameObject Static, Baldi, WarningMarks;
    [SerializeField] private AudioSource TelevisionDevice;
    [SerializeField] private AudioClip mus_Alert, aud_TimesOutBell;
    [SerializeField] private AudioQueueScript audioQueue;

    [Header("Extras")]
    [SerializeField] private bool Markings;
    [SerializeField] private MarkingSoundType markingSound = MarkingSoundType.Alert;
    public enum MarkingSoundType { Alert, Bell }
}