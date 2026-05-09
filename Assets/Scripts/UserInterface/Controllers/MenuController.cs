using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
	public void OnEnable()
	{
		uc.firstButton = firstButton;
		uc.SwitchMenu();
	}

	private void Update()
	{
		if (Input.GetButtonDown("Cancel") && back != null)
		{
			back.SetActive(true);
			gameObject.SetActive(false);
		}
	}

	[Header("UI Controller")]
	[SerializeField] private UIController uc;

    [Header("Buttons")]
	[SerializeField] private Selectable firstButton;
    [SerializeField] private GameObject back;
	
}