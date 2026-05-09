using UnityEngine;

public class ITM_ZestyBar : BaseItem
{
    public override bool OnUse()
    {
        GameControllerScript.Instance.audioDevice.PlayOneShot(aud_Crunch);

        if (AdditionalGameCustomizer.Instance.AnOldRule)
        {
            GameControllerScript.Instance.player.ResetGuilt("eat", 1f);
        }

        GameControllerScript.Instance.player.stamina = GameControllerScript.Instance.player.maxStamina * 2f;
        GameControllerScript.Instance.player.SetStamina(PlayerScript.StaminaChangeMode.Set, ZestyStamina);

        return true;
    }
    
    [SerializeField] private int ZestyStamina = 200;
    [SerializeField] private AudioClip aud_Crunch;
}
