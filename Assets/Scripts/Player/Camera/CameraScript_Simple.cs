using UnityEngine;

public class CameraScript_Simple : MonoBehaviour
{
	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player");
		offset = transform.position - player.transform.position;
	}

	private void Update()
	{
		if (Singleton<InputManager>.Instance.GetActionKey(InputAction.LookBehind))
		{
			lookBehind = 180;
		}
		else
		{
			lookBehind = 0;
		}
	}
	
	private void LateUpdate()
	{
		transform.position = player.transform.position + offset;
		transform.rotation = player.transform.rotation * Quaternion.Euler(0f, lookBehind, 0f);
	}

	private GameObject player;
	private int lookBehind;
	private Vector3 offset;
}
