using UnityEngine;

public class RuleFreeZone : MonoBehaviour
{
    private void Start() => player = FindObjectOfType<PlayerScript>();
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            outside = true;
        }
    }

    private void Update()
    {
        if ((transform.position - player.transform.position).magnitude >= 180f)
        {
            outside = false;
        }
        if (outside)
        {
            if (player.stamina <= 100f)
            { 
                player.stamina += player.staminaRise * Time.fixedDeltaTime;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            outside = false;
        }
    }

    private PlayerScript player;
    private bool outside;
}