using UnityEngine;

public class ITM_Boots : BaseItem
{
    public override bool OnUse()
    {
        GameControllerScript.Instance.player.ActivateBoots();
        Instantiate(bootsPrefab);

        return true;
    }
    
    [SerializeField] private GameObject bootsPrefab;
}
