using TMPro;
using UnityEngine;

public class ScoreScript : MonoBehaviour
{
	private void Start()
	{
		if (PlayerPrefs.GetString("CurrentMode") == "endless")
		{
			scoreText.SetActive(true);
			text.text = "Score:\n" + PlayerPrefs.GetInt("CurrentBooks") + " Notebooks";
		}
	}

	[Header("References")]
	[SerializeField] private GameObject scoreText;
	[SerializeField] private TMP_Text text;
	
}
