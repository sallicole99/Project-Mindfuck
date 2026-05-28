using UnityEngine;

// Attach this to a GameObject with a Box Collider set to "Is Trigger"
// Place and resize it to cover your outdoor texture areas in the scene
public class OutdoorZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<StaminaSystem>()?.EnterOutdoors();
        other.GetComponent<HPSystem>()?.EnterOutdoors();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<StaminaSystem>()?.ExitOutdoors();
        other.GetComponent<HPSystem>()?.ExitOutdoors();
    }
}
