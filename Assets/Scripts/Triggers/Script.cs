using UnityEngine;

public class Script : MonoBehaviour
{
	private void Start()
	{
		audioDevice = GetComponent<AudioSource>();
		audioDevice.ignoreListenerPause = true;
	}

	private void Update()
	{
		if (played && !audioDevice.isPlaying && audioDevice.time < audioDevice.clip.length - 0.1f)
		{
			if (!Application.isEditor)
			{
				UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.Abort);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.name == "Player" & !played)
		{
			audioDevice.Play();
			played = true;
		}
	}

	private AudioSource audioDevice;
	private bool played;
}