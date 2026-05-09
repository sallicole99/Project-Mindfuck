using UnityEngine;

public class FacultyTriggerScript : MonoBehaviour
{
	private void Start() => ps = FindObjectOfType<PlayerScript>();
	
	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			ps.ResetGuilt("faculty", 1f);
		}
	}

	private PlayerScript ps;
}