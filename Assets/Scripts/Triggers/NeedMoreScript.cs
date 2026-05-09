using UnityEngine;

public class NeedMoreScript : MonoBehaviour
{
	private void Start()
	{
		gc = FindObjectOfType<GameControllerScript>();
		audioDevice = GetComponent<AudioSource>();
		tutorDevice = GameObject.Find("TutorBaldi")?.GetComponent<AudioSource>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (gc.notebooks < gc.UnlockAmount & other.CompareTag("Player"))
		{
			if (!audioDevice.isPlaying & !tutorDevice.isPlaying)
			{
				audioDevice.Play();
			}
		}
	}

	private GameControllerScript gc;
    private AudioSource audioDevice, tutorDevice;
}