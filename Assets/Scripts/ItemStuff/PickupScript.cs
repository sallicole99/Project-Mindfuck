using UnityEngine;
using System.Collections.Generic;

public class PickupScript : Interactable
{
    #region Initialization Logic
    public void Start()
    {
        cachedSprites = new Dictionary<int, Sprite>();

        if (PresentMode)
        {
            GetComponentInChildren<SpriteRenderer>().sprite = GameControllerScript.Instance.Present;
            ID = Random.Range(1, 11);
        }

        if (SpawnAtRandom)
        {
            wanderer = FindObjectOfType<AILocationSelectorScript>();
            GameObject Set = GameObject.Find("AI_LocationSelector");
            location = Set.transform;
            location.position = wanderer.SetNewTargetForAgent(null, AILocationSelectorScript.LocationType.Present);
            transform.position = location.position + Vector3.up * 4f;
        }
    }
    #endregion

    #region Player Interaction
    private void UpdateSpriteAndID()
    {
        if (!cachedSprites.ContainsKey(ID))
        {
            Texture itemTexture = ItemManager.Instance.GetItem(ID).BigSprite;
            Sprite itemSprite = Sprite.Create(
                (Texture2D)itemTexture,
                new Rect(0, 0, itemTexture.width, itemTexture.height),
                new Vector2(0.5f, 0.5f), 100);
            cachedSprites.Add(ID, itemSprite);
        }

        GetComponentInChildren<SpriteRenderer>().sprite = cachedSprites[ID];
        gameObject.name = $"Pickup_{ItemManager.Instance.GetItem(ID).Name}";
    }

    public override void Interact()
    {
        GameControllerScript.Instance.audioDevice.PlayOneShot(GameControllerScript.Instance.aud_ItemCollect);

        // ── Currency shortcut ──────────────────────────────────────────
        if (AdditionalGameCustomizer.Instance?.ReworkedCurrency == true && ID == 5)
        {
            AdditionalGameCustomizer.Instance.Cash += 0.25f;
            transform.gameObject.SetActive(false);
            return;
        }

        int      pickedUpID       = ID;
        int      pickedUpAmount   = stackAmount;
        BaseItem pickedUpInstance = GetHeldInstance();

        // ── Case 1: inventory has an empty slot — normal collect ───────
        if (HasEmptySlot())
        {
            if (!DroppedItem)
                transform.gameObject.SetActive(false);
            else
                Destroy(gameObject);

            ItemManager.Instance.CollectItem(pickedUpID, pickedUpInstance, pickedUpAmount, maxStack);
            return;
        }

        // ── Case 2: inventory is full — swap with currently selected slot
        int swapSlot = ItemManager.Instance.ItemSelection;

        // Capture everything from the outgoing slot BEFORE touching the inventory
        int      swappedOutID    = ItemManager.Instance.Inventory[swapSlot].ItemID;
        int      swappedOutStack = ItemManager.Instance.Inventory[swapSlot].StackCount;
        int      swappedOutMax   = ItemManager.Instance.Inventory[swapSlot].MaxStack;
        BaseItem swappedOutItem  = ItemManager.Instance.GetSelectedItemObject();

        // Update this pickup's stack metadata to match what we're ejecting,
        // so if the player picks it back up the correct count transfers back in.
        stackAmount = swappedOutStack;
        maxStack    = swappedOutMax;

        // Hand the outgoing instance to this pickup's transform so it lives in the world
        if (swappedOutItem != null)
            swappedOutItem.transform.SetParent(transform);

        // Update pickup visuals to show the ejected item
        ID = swappedOutID;
        UpdateSpriteAndID();
        transform.gameObject.SetActive(true);

        // ── Write the picked-up item directly into the slot ────────────
        // Bypass CollectItem so it can't accidentally stack into a different slot.
        ItemManager.Instance.ClearItem(swapSlot);
        ItemManager.Instance.Inventory[swapSlot].ItemID     = pickedUpID;
        ItemManager.Instance.Inventory[swapSlot].StackCount = pickedUpAmount;
        ItemManager.Instance.Inventory[swapSlot].MaxStack   = swappedOutMax;

        // Parent the incoming instance under ItemManager (neutral ground),
        // not under this pickup object which now represents a different item.
        if (pickedUpInstance != null)
        {
            pickedUpInstance.transform.SetParent(ItemManager.Instance.transform);
            ItemManager.Instance.Inventory[swapSlot].ItemInstance = pickedUpInstance;
            pickedUpInstance.OnPickup();
        }
        else
        {
            // No live instance came with the pickup — let ItemManager create one
            ItemManager.Instance.Inventory[swapSlot].ItemInstance = null;
        }

        if (ItemManager.Instance.ItemSelection == swapSlot)
            ItemManager.Instance.GetItem(pickedUpID)?.OnSelect();

        ItemManager.Instance.UpdateItemUI();
    }
    #endregion

    #region Utility Methods
    private BaseItem GetHeldInstance() => GetComponentInChildren<BaseItem>();

    private bool HasEmptySlot()
    {
        for (int i = 0; i < ItemManager.Instance.Inventory.Length; i++)
        {
            if (ItemManager.Instance.Inventory[i].ItemID == 0)
                return true;
        }
        return false;
    }

    public bool SlotStuffs(bool trueOrNot)
    {
        for (int i = 0; i < ItemManager.Instance.Inventory.Length; i++)
        {
            if (ItemManager.Instance.Inventory[i].ItemID == 0)
                return trueOrNot;
        }
        return !trueOrNot;
    }
    #endregion

    #region Configuration & State
    [Header("Pickup Settings")]
    [SerializeField] private int ID;
    [SerializeField] private bool PresentMode;
    public bool SpawnAtRandom;

    [Header("Stack Settings")]
    [Tooltip("How many of this item are added to the stack when picked up")]
    public int stackAmount = 1;
    [Tooltip("Maximum stack size allowed for this pickup slot")]
    public int maxStack = 1;

    private static Dictionary<int, Sprite> cachedSprites = new Dictionary<int, Sprite>();
    [HideInInspector] public bool DroppedItem;
    private AILocationSelectorScript wanderer;
    private Transform location;
    #endregion
}