using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverScript : MonoBehaviour
{
	private void Start()
	{
		AudioListener.pause = false;
		image = GetComponent<Image>();
		audioDevice = GetComponent<AudioSource>();
		delay = 5f;
		chance = Random.Range(1f, 99f);
		if (chance < 98f)
		{
			int num = Random.Range(0, images.Length);
			image.sprite = images[num];
		}
		else
		{
			image.sprite = rare;
		}
	}

	private void Update()
	{
		delay -= 1f * Time.deltaTime;
		if (delay <= 0f)
		{
			if (chance < 98f)
			{
				SceneManager.LoadScene(OverScene);
			}
			else
			{
				image.transform.localScale = new Vector3(5f, 5f, 1f);
				image.color = Color.red;
				if (!audioDevice.isPlaying)
				{
					audioDevice.Play();
				}
				if (delay <= -5f)
				{
					Application.Quit();
				}
			}
		}
	}

    [Header("Game Over Settings")]
	[SerializeField] private Sprite[] images = new Sprite[5];
    [SerializeField] private Sprite rare;
    [SerializeField] private string OverScene;

	private Image image;
	private float delay;
	private float chance;
	private AudioSource audioDevice;
}
