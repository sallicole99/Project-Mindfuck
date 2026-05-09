using UnityEngine;

public class ITM_SwingDoorLock : BaseItem
{
    public override bool OnUse()
    {
        if (SendRay("", out RaycastHit Ray, 10))
        {
            if (Ray.collider.CompareTag("SwingingDoor"))
            {
                Ray.collider.gameObject.GetComponent<SwingingDoorScript>().LockDoor(DoorLockTime);
                GameControllerScript.Instance.audioDevice.PlayOneShot(aud_Locked);
                return true;
            }
        }
        return false;
    }
    
    [SerializeField] private int DoorLockTime = 15;
    [SerializeField] private AudioClip aud_Locked;
}
