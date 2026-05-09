using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
	public void StartGame()
	{
		if (currentMode == Mode.Story)
		{
			PlayerPrefs.SetString("CurrentMode", "story");
		}
		else
		{
			PlayerPrefs.SetString("CurrentMode", "endless");
		}
		SceneManager.LoadSceneAsync(LoadScene);
	}

	[Header("Game Mode Settings")]
	[SerializeField] private Mode currentMode;
	[SerializeField] private enum Mode { Story, Endless }

	[Header("Scene Settings")]
	[SerializeField] private string LoadScene;
}