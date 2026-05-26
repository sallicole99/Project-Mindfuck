using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Object Creator/Subtitle Object", order = 2)]
public class subsScriptableObject : ScriptableObject
{
    [Tooltip("for setting the duration via just one audioclip lol")]public AudioClip audioClip;
    [Tooltip("main subtitle stuff,if you have an audio clip attached and the duration is 0 the duration is automaticly the audio clip duration, gets disabled if isTheSubtitlesTimed bool is active")]public subtitlingIt subtitleOption;
    [Tooltip("switch to using the subtitleTimingKeys instead of subtitleOption, might be a pain in the ass to time the audio tho cuz subtitleTimingKeys has no auto convert to audio clip duration")]public bool isTheSubtitlesTimed;
    [Tooltip("yea i think this dosent need any explaination")]public subtitlingIt[] SubtitleTimingKey;
}
    
