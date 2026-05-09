public class ITM_Teleporter : BaseItem
{
    public override bool OnUse()
    {
        GameControllerScript.Instance.StartCoroutine(GameControllerScript.Instance.TeleporterFunction());
        return true;
    }
}
