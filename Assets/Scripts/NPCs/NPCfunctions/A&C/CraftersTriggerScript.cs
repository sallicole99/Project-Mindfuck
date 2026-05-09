using UnityEngine;

public class CraftersTriggerScript : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && crafters.isActiveAndEnabled)
		{
			crafters.GiveLocation(goTarget.position, false);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player") && crafters.isActiveAndEnabled)
		{
			crafters.GiveLocation(fleeTarget.position, true);
		}
	}

	[Header("Target Locations")]
	[SerializeField] private Transform goTarget;
    [SerializeField] private Transform fleeTarget;

    [Header("Sock Puppet")]
	[SerializeField] private CraftersScript crafters;
}