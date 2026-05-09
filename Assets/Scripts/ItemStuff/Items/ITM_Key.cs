using UnityEngine;

public class ITM_Key : BaseItem
{
    public override bool OnUse()
    {
        if (SendRay("Door", out RaycastHit Ray))
        {
            DoorScript doorComponent = Ray.collider.gameObject.GetComponent<DoorScript>();
            if (doorComponent != null && doorComponent.DoorLocked)
            {
                doorComponent.UnlockDoor();
                doorComponent.OpenDoor(3);
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        return true;
    }
}