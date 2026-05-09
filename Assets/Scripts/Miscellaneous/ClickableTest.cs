using UnityEngine;

public class ClickableTest : MonoBehaviour
{
	private void Start()
	{
		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && Time.timeScale != 0f && Vector3.Distance(player.position, transform.position) < 10)
        {
            if (Sych.ScreenRaycastMatchesTag("Notebook", out RaycastHit hit, 10f) && hit.transform == transform)
            {
                gameObject.SetActive(false);
            }
        }
	}

	private Transform player;
}