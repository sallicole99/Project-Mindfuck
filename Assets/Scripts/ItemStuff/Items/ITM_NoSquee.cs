using UnityEngine;

public class ITM_NoSquee : BaseItem
{
    public override bool OnUse()
    {
        Vector3 playerPosition = GameControllerScript.Instance.player.transform.position;
        Vector3 snappedPosition = Sych.SnapToGrid(playerPosition);

        Instantiate(WDNSModel, snappedPosition, GameControllerScript.Instance.cameraTransform.rotation);
        GameControllerScript.Instance.audioDevice.PlayOneShot(aud_Spray);

        return true;
    }

    [SerializeField] private GameObject WDNSModel;
    [SerializeField] protected AudioClip aud_Spray;
}