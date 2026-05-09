using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TutorScript : MonoBehaviour
{
    private void Start()
    {
        tutorSource.PlayOneShot(aud_Hi);

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObject.transform;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        tutorSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !triggeredCounting)
        {
            tutorAnimation.Rebind();
            tutorSource.Stop();
            tutorAnimation.Play("BaldiWave", -1, 0f);
            tutorSource.PlayOneShot(aud_Hi);
        }

        if (Countdown && !triggeredCounting)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance > spoopDistance)
            {
                triggeredCounting = true;
                StartCoroutine(PlayCountdown());
            }
        }
    }

    private IEnumerator PlayCountdown()
    {
        while (tutorAnimation.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f && !tutorAnimation.IsInTransition(0))
        {
            yield return null;
        }
        tutorAnimation.enabled = false;

        foreach (AudioClip clip in countdownClips)
        {
            yield return StartCoroutine(DelayWithSpriteChange());

            spriteRenderer.sprite = talkingSprite;
            tutorSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }

        if (ReadyOrNot != null)
        {
            yield return StartCoroutine(DelayWithSpriteChange());

            spriteRenderer.sprite = talkingSprite;
            tutorSource.PlayOneShot(ReadyOrNot);
            yield return new WaitForSeconds(ReadyOrNot.length);
        }

        GameControllerScript.Instance.ActivateSpoopMode();
        gameObject.SetActive(false);
    }

    private IEnumerator DelayWithSpriteChange()
    {
        float rand = Random.value;
        if (rand < peekChance)
        {
            spriteRenderer.sprite = peekingSprite;
        }
        else
        {
            spriteRenderer.sprite = closedSprite;
        }

        yield return new WaitForSeconds(delayClips);
    }

    [Header("Basic")]
    [SerializeField] private Animator tutorAnimation;
    [SerializeField] private AudioClip aud_Hi, ReadyOrNot;
    [SerializeField] private List<AudioClip> countdownClips;
    [SerializeField] private bool Countdown;

    [Header("Sprite States")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite talkingSprite, peekingSprite;

    private float spoopDistance = 75f;
    private float delayClips = 1f;
    private bool triggeredCounting = false;
    private float peekChance = 0.15f;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    [HideInInspector] public AudioSource tutorSource;
}