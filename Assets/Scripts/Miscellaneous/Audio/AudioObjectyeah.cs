using System;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Object Creator/Audio Object", order = 1)]
public class AudioObjectyeah : ScriptableObject
{
    public AudioClip audClip;
    [Tooltip("do i even need to explain"),Range(0f, 1f)] public float volume;
    [Tooltip("what sound type is it gang")] public SoundAudioType SoundTypeWahh;
    [Tooltip("im not gonna explain this is too self explanatory")] public bool HasSubtitle;
    [Tooltip("force set the audio mixer, pretty useless imo")] public AudioMixerGroup ForcedSetAudiomixer;
    [Tooltip("the   subtitle")] public subsScriptableObject subti;
}

public enum SoundAudioType
{		
    None = 0,
	Sound = 1,
	Voice = 2,
	Music = 3,
    SoundAlt = 4,
	VoiceAlt = 5,
	MusicAlt = 6
}
    
