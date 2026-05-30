using TMPro;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    #region Singleton & Initialization
    public void Awake()
    {
        Instance = this;
        IndexItems();
    }
    #endregion

    #region Input Handling
    private void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        for (int i = 0; i < KeyIndex.Length; i++)
        {
            bool keyCode = Singleton<InputManager>.Instance.GetActionKey(InputAction.Slot0 + 0 + i);
            if (keyCode)
            {
                ExecuteItem(Inventory[ItemSelection].ItemID, ExecutionType.Deselect);
                ExecuteItem(Inventory[i].ItemID, ExecutionType.Select);
                ItemSelection = i;
                UpdateItemUI();
                break;
            }
        }

        if (Input.GetMouseButtonDown(1) || Singleton<InputManager>.Instance.GetActionKey(InputAction.UseItem))
        {
            int CurrItem = GetSelectedItem();
            bool ShouldDestroy = ExecuteItem(CurrItem);
            BaseItem SelectedItemObject = GetSelectedItemObject();

            if (CurrItem == GetSelectedItem())
            {
                if (!ShouldDestroy)
                {
                    UpdateItemUI();
                    return;
                }

                SelectedItemObject.Uses--;
                if (SelectedItemObject.Uses <= 0)
                {
                    ExecuteItem(GetSelectedItem(), ExecutionType.Deselect);
                    if (Inventory[ItemSelection].ItemInstance != null)
                    {
                        Destroy(Inventory[ItemSelection].ItemInstance.gameObject);
                    }

                    Inventory[ItemSelection].StackCount--;
                    if (Inventory[ItemSelection].StackCount <= 0)
                    {
                        ClearItem(ItemSelection);
                    }
                    else
                    {
                        SelectedItemObject.Uses = 1;
                    }
                }
            }

            UpdateItemUI();
        }

        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0)
        {
            UpdateItemSelection(scrollDelta > 0 ? -1 : 1);
        }
    }
    #endregion

    #region Item Execution & Inventory Management
    private void IndexItems()
    {
        BaseItem[] FoundItemObjects = GetComponentsInChildren<BaseItem>();
        for (int i = 0; i < FoundItemObjects.Length; i++)
        {
            BaseItem item = FoundItemObjects[i];
            Items.Add(item.Name, item);

            if (item.ItemID == 0 && item.Name != "Nothing")
                Debug.LogWarning($"[ItemManager] BaseItem '{item.Name}' has ItemID 0 — assign a unique ItemID in the Inspector.");
            else if (ItemIDMap.ContainsKey(item.ItemID))
                Debug.LogError($"[ItemManager] Duplicate ItemID {item.ItemID} on '{item.Name}' and '{ItemIDMap[item.ItemID]}' — IDs must be unique.");
            else
                ItemIDMap.Add(item.ItemID, item.Name);
        }

        Array.Resize(ref Inventory, ItemImages.Count);
        Array.Resize(ref KeyIndex, Inventory.Length);

        for (int i = 0; i < Inventory.Length; i++)
        {
            Inventory[i].StackCount = 0;
            Inventory[i].MaxStack   = (maxStackPerSlot != null && i < maxStackPerSlot.Length && maxStackPerSlot[i] > 0)
                                      ? maxStackPerSlot[i] : 1;
        }

        UpdateItemUI();
    }

    private bool ExecuteItem(int ID, ExecutionType type = ExecutionType.Use)
    {
        if (ID == 0) return false; // empty slot — silently skip

        BaseItem item = GetItem(ID);
        if (item == null)
        {
            Debug.LogError($"[ItemManager] ExecuteItem: no item found for ID {ID}, type {type}");
            return false;
        }

        switch (type)
        {
            case ExecutionType.Use:      return item.OnUse();
            case ExecutionType.Pickup:   item.OnPickup();   break;
            case ExecutionType.Select:   item.OnSelect();   break;
            case ExecutionType.Deselect: item.OnDeselect(); break;
        }
        return false;
    }

    private void UpdateItemSelection(int changeAmount)
    {
        ExecuteItem(Inventory[ItemSelection].ItemID, ExecutionType.Deselect);
        ItemSelection = (ItemSelection + changeAmount + Inventory.Length) % Inventory.Length;
        ExecuteItem(Inventory[ItemSelection].ItemID, ExecutionType.Select);
        UpdateItemUI();
    }

    public void ClearItem(int index)
    {
        Inventory[index].ItemID       = 0;
        Inventory[index].ItemInstance = null;
        Inventory[index].StackCount   = 0;
        Inventory[index].MaxStack     = (maxStackPerSlot != null && index < maxStackPerSlot.Length && maxStackPerSlot[index] > 0)
                                        ? maxStackPerSlot[index] : 1;
    }

    private void SetItem(int index, int itemID, BaseItem item = null, int stackCount = 1)
    {
        // Parent instance to ItemManager itself — never to the shared template object.
        // Parenting to the template causes instances from different swaps to get cross-nested,
        // making GetSelectedItemObject() return the wrong BaseItem (wrong name, wrong stats).
        if (item != null)
            item.transform.SetParent(transform);

        // Deselect whatever is currently in this slot before replacing it
        if (Inventory[index].ItemID != 0)
            ExecuteItem(Inventory[index].ItemID, ExecutionType.Deselect);

        int preservedMax              = Inventory[index].MaxStack > 0 ? Inventory[index].MaxStack : 1;
        Inventory[index].ItemID       = itemID;
        Inventory[index].ItemInstance = item;
        Inventory[index].StackCount   = stackCount;
        Inventory[index].MaxStack     = preservedMax;

        // Only create a fresh instance when one wasn't already provided.
        // If an instance was handed in (e.g. from a pickup swap), don't overwrite it
        // with a brand-new one — that would reset stack count and Uses to defaults.
        if (item == null)
            CreateItemInstance(index);

        ExecuteItem(Inventory[index].ItemID, ExecutionType.Pickup);
        if (ItemSelection == index)
            ExecuteItem(Inventory[index].ItemID, ExecutionType.Select);
    }
    #endregion

    #region UI Management
    public void UpdateItemUI()
    {
        for (int i = 0; i < ItemImages.Count; i++)
        {
            ItemImageBGs[i].color = Color.white;

            // Use stable ID lookup — ElementAt(id) uses insertion order, not item ID,
            // which caused wrong sprites/names when item order didn't match their IDs.
            BaseItem slotItem = GetItem(Inventory[i].ItemID);
            if (slotItem != null)
            {
                ItemImages[i].texture = slotItem.SmallSprite;
                ItemImages[i].color   = Color.white;
            }
            else
            {
                ItemImages[i].texture = null;
                ItemImages[i].color   = Color.clear;
            }

            if (i < StackCountTexts.Count && StackCountTexts[i] != null)
            {
                if (Inventory[i].ItemID != 0 && Inventory[i].StackCount > 1)
                {
                    StackCountTexts[i].gameObject.SetActive(true);
                    StackCountTexts[i].text = Inventory[i].StackCount.ToString();
                }
                else
                {
                    StackCountTexts[i].gameObject.SetActive(false);
                }
            }
        }

        BaseItem SelectedItem = GetSelectedItemObject();
        if (SelectedItem != null)
        {
            ItemNameText.text  = $"{SelectedItem.Name}";
            ItemNameText.color = SelectedItem.NameColor;
            if (SelectedItem.Uses > 1)
                ItemNameText.text += $" ({SelectedItem.Uses})";
        }
        else
        {
            // Empty slot — show the Nothing item's name and color directly
            BaseItem nothingItem = GetItem("Nothing");
            if (nothingItem != null)
            {
                ItemNameText.text  = nothingItem.Name;
                ItemNameText.color = nothingItem.NameColor;
            }
            else
            {
                ItemNameText.text = string.Empty;
            }
        }

        ItemImageBGs[ItemSelection].color = SelectionColor;
    }
    #endregion

    #region Function Handling
    public BaseItem GetItem(string name) => Items.ContainsKey(name) ? Items[name] : null;
    public BaseItem GetItem(int id)
    {
        if (id == 0) return null;
        if (ItemIDMap.TryGetValue(id, out string name))
            return GetItem(name);
        Debug.LogWarning($"[ItemManager] GetItem: no item registered with ID {id}. Check BaseItem.ItemID in the Inspector.");
        return null;
    }

    public void AddItem(BaseItem item)
    {
        if (item != null && !Items.ContainsKey(item.name)) { Items.Add(item.name, item); return; }
        Debug.LogWarning("Attempted to add an item that was either null or was already apart of the items dictionary");
    }

    public void RemoveItem(string name)
    {
        if (Items.ContainsKey(name)) { Items.Remove(name); return; }
        Debug.LogWarning("Attempted to remove an item that wasn't apart of the items dictionary");
    }

    public void RemoveItem(BaseItem item) => RemoveItem(item.name);
    public int GetSelectedItem() => Inventory[ItemSelection].ItemID;
    public bool IsInventoryFull() => Inventory.All(i => i.ItemID != 0 && i.StackCount >= i.MaxStack);
    public bool HasNoItems() => Inventory.All(i => i.ItemID == 0);

    public BaseItem GetSelectedItemObject()
    {
        if (Inventory[ItemSelection].ItemID != 0 && Inventory[ItemSelection].ItemInstance == null)
        {
            CreateItemInstance();
            return Inventory[ItemSelection].ItemInstance?.GetComponent<BaseItem>();
        }
        return Inventory[ItemSelection].ItemInstance != null
            ? Inventory[ItemSelection].ItemInstance
            : GetItem(GetSelectedItem());
    }
    #endregion

    #region Item Instances & Collection
    private void CreateItemInstance(int? at = null)
    {
        int index = at ?? ItemSelection;
        if (Inventory[index].ItemID == 0) return;
        if (Inventory[index].ItemInstance == null)
        {
            BaseItem itemobj = GetItem(Inventory[index].ItemID);
            if (itemobj == null)
            {
                Debug.LogError($"[ItemManager] CreateItemInstance: could not find item with ID {Inventory[index].ItemID}. Make sure BaseItem.ItemID is set in the Inspector.");
                return;
            }
            GameObject NewInstance = Instantiate(itemobj.gameObject, transform);
            NewInstance.name = itemobj.gameObject.name;
            Inventory[index].ItemInstance = NewInstance.GetComponent<BaseItem>();
        }
    }

    public void CollectItem(int ItemID, BaseItem instance = null, int stackAmount = 1, int maxStack = 1)
    {
        int remaining = stackAmount;

        // Pass 1: top up existing slots that already hold this item
        for (int i = 0; i < Inventory.Length && remaining > 0; i++)
        {
            if (Inventory[i].ItemID != ItemID) continue;
            if (Inventory[i].StackCount >= Inventory[i].MaxStack) continue;

            int space = Inventory[i].MaxStack - Inventory[i].StackCount;
            int add   = Mathf.Min(remaining, space);
            Inventory[i].StackCount += add;
            remaining -= add;
        }

        // Pass 2: spill remainder into empty slots
        for (int i = 0; i < Inventory.Length && remaining > 0; i++)
        {
            if (Inventory[i].ItemID != 0) continue;

            int slotMax = Inventory[i].MaxStack > 0 ? Inventory[i].MaxStack : 1;
            int add     = Mathf.Min(remaining, slotMax);
            SetItem(i, ItemID, instance, add);
            remaining -= add;
        }

        UpdateItemUI();
    }

    public void ReplaceCurrentItem(int ItemID)
    {
        if (Inventory[ItemSelection].ItemInstance != null)
            Destroy(Inventory[ItemSelection].ItemInstance.gameObject);
        SetItem(ItemSelection, ItemID);
        UpdateItemUI();
    }

    // ─── Spawns a world pickup without touching the inventory slot ────
    private void SpawnPickup(int index, int amount)
    {
        var item = Inventory[index];
        if (item.ItemID == 0) return;

        BaseItem itemToDrop = item.ItemInstance != null ? item.ItemInstance : GetItem(item.ItemID);
        Vector3 spawnPosition = GameControllerScript.Instance.player.ItemDropLocation.transform.position;

        GameObject droppedItem = new GameObject($"Pickup_{itemToDrop.Name}")
        {
            transform = { position = spawnPosition },
            tag = "Item"
        };

        var pickup = droppedItem.AddComponent<PickupScript>();
        pickup.DroppedItem  = true;
        pickup.stackAmount  = amount;
        pickup.maxStack     = amount;

        typeof(PickupScript).GetField("ID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(pickup, item.ItemID);
        pickup.GetType().GetField("PresentMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(pickup, false);

        var collider = droppedItem.AddComponent<CapsuleCollider>();
        collider.isTrigger = true;
        collider.center    = new Vector3(0, 1, 0);
        collider.radius    = 1.5f;
        collider.height    = 2f;

        GameObject spriteObject = new GameObject("Sprite")
        {
            transform = { parent = droppedItem.transform, localPosition = Vector3.zero, localScale = new Vector3(2f, 2f, 2f) }
        };

        SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        if (itemToDrop.BigSprite is Texture2D texture)
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
        else
            Debug.LogWarning("BigSprite is not a Texture2D, cannot create Sprite.");

        spriteRenderer.material = GameControllerScript.Instance.SpriteRenderer;
        spriteObject.AddComponent<Billboard>();
        spriteObject.AddComponent<PickupAnimationScript>();
        spriteObject.AddComponent<SpriteColorManager>();
    }

    // ─── Drop 1 from stack, spawn pickup, clear slot if empty ────────
    public void DropOneFromStack(int index)
    {
        if (Inventory[index].ItemID == 0 || Inventory[index].StackCount <= 0) return;

        SpawnPickup(index, 1);
        Inventory[index].StackCount--;

        if (Inventory[index].StackCount <= 0)
            ClearItem(index);

        UpdateItemUI();
    }

    // ─── Drop bulk amount from stack, spawn one pickup with that count ─
    public void DropBulkFromStack(int index, int amount)
    {
        if (Inventory[index].ItemID == 0 || Inventory[index].StackCount <= 0) return;

        int dropAmount = Mathf.Min(amount, Inventory[index].StackCount);
        SpawnPickup(index, dropAmount);
        Inventory[index].StackCount -= dropAmount;

        if (Inventory[index].StackCount <= 0)
            ClearItem(index);

        UpdateItemUI();
    }

    // ─── Original DropItem — drops entire slot as one pickup ──────────
    public void DropItem(int index)
    {
        var item = Inventory[index];
        if (item.ItemID == 0 || item.ItemInstance == null) return;

        SpawnPickup(index, item.StackCount);

        BaseItem itemToDrop = item.ItemInstance;
        itemToDrop.transform.SetParent(GameObject.Find($"Pickup_{itemToDrop.Name}")?.transform);
        itemToDrop.gameObject.SetActive(true);

        ClearItem(index);
        UpdateItemUI();
    }
    #endregion

    #region Change References
    public void ChangeReferences(List<RawImage> itemImages, List<Image> itemImgBgs, List<TextMeshProUGUI> stackTexts = null)
    {
        ItemImages   = itemImages;
        ItemImageBGs = itemImgBgs;
        if (stackTexts != null) StackCountTexts = stackTexts;

        for (int i = 0; i < Inventory.Length; i++)
        {
            Inventory[i].MaxStack = (maxStackPerSlot != null && i < maxStackPerSlot.Length && maxStackPerSlot[i] > 0)
                                    ? maxStackPerSlot[i] : 1;
        }
    }
    #endregion

    #region Nested Types
    [Serializable]
    public struct HeldItem
    {
        public int ItemID;
        public BaseItem ItemInstance;
        public int StackCount;
        public int MaxStack;
    }
    private enum ExecutionType { Use, Pickup, Select, Deselect }
    #endregion

    #region Fields & Serialized
    private Dictionary<string, BaseItem> Items    = new Dictionary<string, BaseItem>();
    private Dictionary<int, string>      ItemIDMap = new Dictionary<int, string>(); // stable int ID → item name
    public HeldItem[] Inventory;
    private KeyCode[] KeyIndex = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };

    [Header("UI References")]
    [SerializeField] private List<RawImage> ItemImages = new List<RawImage>();
    [SerializeField] private List<Image> ItemImageBGs = new List<Image>();
    [SerializeField] private List<TextMeshProUGUI> StackCountTexts = new List<TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI ItemNameText;
    [SerializeField] private Color SelectionColor = Color.red;

    [Header("Stack Settings")]
    [Tooltip("Max stack size per inventory slot — set one value per slot")]
    [SerializeField] public int[] maxStackPerSlot;

    [HideInInspector] public int ItemSelection = 0;
    public static ItemManager Instance;
    #endregion
}