using UnityEngine;

public class ITM_Tape : BaseItem
{
    public override bool OnUse()
    {
        if (SendRay("", out RaycastHit Ray, 15))
        {
            if (Ray.collider.CompareTag("TapePlayer"))
            {
                Ray.collider.gameObject.GetComponent<TapePlayerScript>().Play();
                GameControllerScript.Instance.audioDevice.PlayOneShot(aud_Insert);
                return true;
            }
        }
        return false;
    }

    [SerializeField] private AudioClip aud_Insert;
}
