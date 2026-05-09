using UnityEngine;

public class VendingMachineScript : MonoBehaviour
{
    #region Initialization
    private void Start()
    {
        if (crazyMode && isOutOfGoods)
        {
            Debug.LogWarning("Conflict: CrazyMode and isOutOfGoods both true. Disabling CrazyMode.");
            crazyMode = false;
        }

        if (crazyMode)
        {
            VendingFront.material = CrazyFront;
            itemID = Random.Range(1, 11);
        }
    }
    #endregion

    #region Public Actions
    public void DispenseItem()
    {
        if (AdditionalGameCustomizer.Instance.ReworkedCurrency)
        {
            if (!ItemManager.Instance.IsInventoryFull())
            {
                ItemManager.Instance.CollectItem(itemID);
            }
            else
            {
                Debug.Log("Inventory full. Cannot collect item.");
            }
        }
        else
        {
            ItemManager.Instance.ReplaceCurrentItem(itemID);
        }

        if (isOutOfGoods)
        {
            HandleOutOfGoodsState();
            return;
        }

        if (crazyMode)
        {
            itemID = Random.Range(1, 11);
        }
    }

    public void RestockVendingMachine()
    {
        if (!gameObject.CompareTag("Untagged")) return;

        gameObject.tag = "VendingMachine";
        VendingFront.material = NormalFront;
    }
    #endregion

    #region State Handlers
    private void HandleOutOfGoodsState()
    {
        if (!crazyMode)
        {
            VendingFront.material = outOfFront;
            gameObject.tag = "Untagged";
        }
    }
    #endregion

    #region Serialized Configuration
    [Header("Settings")]
    [SerializeField] private int itemID = 1;
    [SerializeField] private bool crazyMode = false, isOutOfGoods = true;

    [Header("Materials")]
    [SerializeField] private Material CrazyFront;
    [SerializeField] private Material outOfFront, NormalFront;
    [SerializeField] private MeshRenderer VendingFront;
    #endregion
}