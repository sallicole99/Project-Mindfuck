using UnityEngine;
using System.Collections;

public class ExitButtonScript : MonoBehaviour
{
	public void ExitGame()
	{
		BaldiSource.Stop();
		BaldiSource.PlayOneShot(aud_Thanks);
		Sych.SetCursorLock(true);
		StartCoroutine(WaitForAudio());
	}

	private IEnumerator WaitForAudio()
	{
		while (BaldiSource.isPlaying)
		{
			yield return null;
		}
		Application.Quit();
		yield break;
	}

	[Header("References")]
    [SerializeField] private AudioSource BaldiSource;
	[SerializeField] private AudioClip aud_Thanks;
}