using TMPro;
using UnityEngine;

public class EndlessTextScript : MonoBehaviour
{
	private void Start() => text.text = string.Concat(text.text, "\nHigh Score: ", PlayerPrefs.GetInt("HighBooks"), " Notebooks");
	
	[Header("References")]
	[SerializeField] private TMP_Text text;
}
