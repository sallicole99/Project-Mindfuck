using UnityEngine;

public class ExitTriggerScript : MonoBehaviour
{
	private void Start()
	{
		gc = FindObjectOfType<GameControllerScript>();
		em = FindObjectOfType<EndingManager>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (gc.notebooks >= gc.maxNotebooks & other.gameObject.CompareTag("Player"))
		{
			if (gc.failedNotebooks >= gc.maxNotebooks)
			{
				em.LoadSecretEnding();
			}
			else
			{
				em.LoadNormalResults();
			}
		}
	}

	private EndingManager em;
	private GameControllerScript gc;
}
