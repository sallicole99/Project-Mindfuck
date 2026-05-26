using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using GrayCreature.YuriArchive.subtitlesYaoiYuriLiveReaction;

public class AudioManagerLiveReaction : MonoBehaviour
{
    private void OnEnable() => InitialiUhh();
    private void OnDestroy() => disabled();
    private void OnDisable()=> disabled();
    private void disabled()
    {
        if (!AudioManStandalone)
        {
            if (totalIds >= TBSSubMan.SubtitleTotalId) Array.Resize(ref TBSSubMan.Instance.subsObjectIdSon, TBSSubMan.SubtitleTotalId + (totalIds - 1));
        }
    }

    #region PlaybackControl
    public void InitialiUhh()
    {
        if (!AudioManStandalone)
        {
            totalIds++;
            sourceId = totalIds;
            if (totalIds >= TBSSubMan.SubtitleTotalId) Array.Resize(ref TBSSubMan.Instance.subsObjectIdSon, TBSSubMan.SubtitleTotalId + (totalIds + 1));
        }
        if (audioDevice == null) audioDevice = GetComponent<AudioSource>();
        ClearQueue(true);
        if (StartingLoop) SetLoop(true);
        if (PlayAtAwake && thoseWhoObject != null) 
        {
            
            if (!PlayAtAwakeQueue) PlaySingleClip(thoseWhoObject);
            else QueueAudio(thoseWhoObject);
        }
    }
    
    public void PlaySingleClip(AudioObjectyeah obje)
    {
        if (obje != null)  
        {
            if (obje.ForcedSetAudiomixer != null) audioDevice.outputAudioMixerGroup = obje.ForcedSetAudiomixer;
            audioDevice.volume = obje.volume;
            audioDevice.PlayOneShot(obje.audClip);
            if (obje.subti != null && obje.HasSubtitle && !AudioManStandalone) makesub(obje.subti,ForceSubPos ?SubPosForced : obje.subti.subtitleOption.twoDeePosition);
        }   
    }
    private void Update()
    {
        if (queuedAudios.Count > 0 && (!AudioListener.pause || audioDevice.ignoreListenerPause) && !audioDevice.isPlaying) PlayNext();
    }
    public void QueueAudio(AudioObjectyeah obje)
    {
        if (obje != null)  
        {
            if (obje.ForcedSetAudiomixer != null) audioDevice.outputAudioMixerGroup = obje.ForcedSetAudiomixer;
            queuedAudios.Add(obje);
            if (obje.subti != null && obje.HasSubtitle && !AudioManStandalone) queuedSubtitles.Add(obje.subti);
        }
    }

    public void ClearQueue(bool EndCurrent = false)
    {
        queuedAudios.Clear();
        queuedSubtitles.Clear();
        if (EndCurrent)
        {
            audioDevice.volume = 1;
            loop = false;
            audioDevice.loop = false;
            audioDevice.mute = false;
            audioDevice.pitch = 1;
            if (!AudioManStandalone)TBSSubMan.Instance.endSubtitle(sourceId);
            audioDevice.Stop();
        }
    }

    private void PlayNext()
    {
        if (queuedAudios[0] != null) 
        {
            audioDevice.volume = queuedAudios[0].volume;
            audioDevice.clip = queuedAudios[0].audClip;
            audioDevice.Play();
            queuedAudios.RemoveAt(0);
            SetLoop(loop);
        }
        if (queuedSubtitles.Count > 0) PlaySubtitles();
    }
    private void PlaySubtitles()
    {
        if (queuedSubtitles[0] != null) 
        {
            if (queuedSubtitles[0] == null) return; //FUCK YOU
            makesub(queuedSubtitles[0],ForceSubPos ?SubPosForced : queuedSubtitles[0].subtitleOption.twoDeePosition);
            queuedSubtitles.RemoveAt(0);
        }
    }
    #endregion

    #region uhh
    public void makesub(subsScriptableObject sub, Vector2 hi) => TBSSubMan.Instance.summonLeSubtitle(sub.subtitleOption, sourceId, sub, audioDevice, hi);
    #endregion

    #region AudioEffects
    public void SetAudioTime(float time) => audioDevice.time = time;
    public void SetIgnoreListenerPause(bool toggle) => audioDevice.ignoreListenerPause = toggle;
    public void SetMute(bool toggle) => audioDevice.mute = toggle;
    public void SetPitch(float pitch) => audioDevice.pitch = pitch;
    public void SetVolume(float vol) => audioDevice.volume = vol;

    public void SetLoop(bool toggle)
    {
        loop = toggle;
		if (queuedAudios.Count == 0 && toggle)audioDevice.loop = true;
		else audioDevice.loop = false;
    }
    #endregion

    #region References

    public bool PlayAtAwake,PlayAtAwakeQueue,StartingLoop;
    public AudioObjectyeah thoseWhoObject;
    public bool ForceSubPos,loop;
    public Vector3 SubPosForced;
    public int sourceId;
	public static int totalIds = 1;
    public AudioSource audioDevice;
    public List<AudioObjectyeah> queuedAudios = new List<AudioObjectyeah>();
    public List<subsScriptableObject> queuedSubtitles = new List<subsScriptableObject>();
    [Header("Standalones stuff (disables the requirement for subtitle)")]
    public bool AudioManStandalone;
    public AudioMixerGroup[] MixerOverrides;
    #endregion
}