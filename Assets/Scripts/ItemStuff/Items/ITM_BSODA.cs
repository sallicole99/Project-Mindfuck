using UnityEngine;

public class ITM_BSODA : BaseItem
{
    public override bool OnUse()
    {
        Vector3 playerPosition = GameControllerScript.Instance.player.transform.position;
        Vector3 snappedPosition = Sych.SnapToGrid(playerPosition);

        Instantiate(bsodaSpray, snappedPosition, GameControllerScript.Instance.cameraTransform.rotation);

        GameControllerScript.Instance.player.ResetGuilt("drink", 1f);
        GameControllerScript.Instance.audioDevice.PlayOneShot(aud_Soda);
        return true;
    }
    
    [SerializeField] private GameObject bsodaSpray;
    [SerializeField] private AudioClip aud_Soda;
}
