using UnityEngine;
using System.Collections.Generic;

public class LifetimeScript : MonoBehaviour
{
    #region Initialization & Cleanup
    private void Start() => ToggleAudioSourcesInRange(true);

    private void OnDestroy()
    {
        ToggleAudioSourcesInRange(false);
        disabledAudioSources.Clear();
    }
    #endregion

    #region Per-Frame Logic
    private void Update()
    {
        if (lifetime.CountdownWithDeltaTime() == 0)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Trigger Handlers
    private void OnTriggerEnter(Collider character)
    {
        if (character.CompareTag("NPC"))
        {
            ToggleAudioSource(character.GetComponent<AudioSource>(), true);
        }
    }

    private void OnTriggerExit(Collider character)
    {
        if (character.CompareTag("NPC"))
        {
            ToggleAudioSource(character.GetComponent<AudioSource>(), false);
        }
    }
    #endregion

    #region Audio Management
    private void ToggleAudioSourcesInRange(bool mute)
    {
        var colliders = Physics.OverlapBox(transform.position, new Vector3(25, 1, 25), Quaternion.identity);
        foreach (var collider in colliders)
        {
            ToggleAudioSource(collider.GetComponent<AudioSource>(), mute);
        }
    }

    private void ToggleAudioSource(AudioSource source, bool mute)
    {
        if (source == null) return;

        bool isCurrentlyMuted = disabledAudioSources.Contains(source);
        if (isCurrentlyMuted != mute)
        {
            source.mute = mute;

            if (mute)
            {
                disabledAudioSources.Add(source);
            }
            else
            {
                disabledAudioSources.Remove(source);
            }
        }
    }
    #endregion

    #region Serialized Configuration
    [Header("Lifetime Settings")]
    [SerializeField] private float lifetime = 300f;

    [Header("Audio Management")]
    [SerializeField] private List<AudioSource> disabledAudioSources = new List<AudioSource>();
    #endregion
}