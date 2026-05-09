using UnityEngine;

public class ITM_Quarter : BaseItem
{
    public override bool OnUse()
    {
        if (SendRay("", out RaycastHit Ray, 10))
        {
            AudioSource audioDevice = GameControllerScript.Instance.audioDevice;

            if (Ray.collider.CompareTag("VendingMachine"))
            {
                VendingMachineScript vendingMachine = Ray.collider.GetComponent<VendingMachineScript>();
                if (vendingMachine != null)
                {
                    audioDevice.PlayOneShot(aud_Drop);
                    vendingMachine.DispenseItem();
                }
                return false;
            }

            if (Ray.collider.CompareTag("Phone"))
            {
                TapePlayerScript tapePlayer = Ray.collider.GetComponent<TapePlayerScript>();
                if (tapePlayer != null)
                {
                    tapePlayer.Play();
                    audioDevice.PlayOneShot(aud_Drop);
                }
                return true;
            }
        }
        return false;
    }
    
    [SerializeField] private AudioClip aud_Drop;
}
