using UnityEngine;

public class AmbienceScript : MonoBehaviour
{
    #region Unity Lifecycle
    private void Awake() => audioDevice = GetComponent<AudioSource>();
    #endregion

    #region AmbiencePlaybackLogic
    public void PlayAudio()
    {
        int num = (int)Random.Range(0f, 49f);
        if (!audioDevice.isPlaying & num == 0)
        {
            transform.position = aiLocation.position;
            int num2 = (int)Random.Range(0f, sounds.Length - 1);
            audioDevice.PlayOneShot(sounds[num2]);
        }
    }
    #endregion

    #region SerializedConfiguration
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] sounds;

    [Header("Location Settings")]
    [SerializeField] private Transform aiLocation;
    
    private AudioSource audioDevice;
    #endregion
}