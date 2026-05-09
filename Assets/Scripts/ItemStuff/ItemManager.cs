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

                    ClearItem(ItemSelection);
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
            Items.Add(FoundItemObjects[i].Name, FoundItemObjects[i]);
        }

        Array.Resize(ref Inventory, ItemImages.Count);
        Array.Resize(ref KeyIndex, Inventory.Length);

        UpdateItemUI();
    }

    private bool ExecuteItem(int ID, ExecutionType type = ExecutionType.Use)
    {
        BaseItem item = GetItem(ID);
        if (item == null)
        {
            Debug.LogError($"Attempted to execute item with ID {ID} and type {type}, but GetItem returned null");
            return false;
        }

        switch (type)
        {
            case ExecutionType.Use:
                return item.OnUse();
            case ExecutionType.Pickup:
                item.OnPickup();
                break;
            case ExecutionType.Select:
                item.OnSelect();
                break;
            case ExecutionType.Deselect:
                item.OnDeselect();
                break;
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
        Inventory[index].ItemID = 0;
        Inventory[index].ItemInstance = null;
    }

    private void SetItem(int index, int itemID, BaseItem item = null)
    {
        item?.transform.SetParent(GetItem(itemID).transform);

        ExecuteItem(Inventory[ItemSelection].ItemID, ExecutionType.Deselect);

        Inventory[index].ItemID = itemID;
        Inventory[index].ItemInstance = item;

        CreateItemInstance(index);

        ExecuteItem(Inventory[index].ItemID, ExecutionType.Pickup);
        if (ItemSelection == index)
        {
            ExecuteItem(Inventory[index].ItemID, ExecutionType.Select);
        }
    }
    #endregion

    #region UI Management
    public void UpdateItemUI()
    {
        for (int i = 0; i < ItemImages.Count; i++)
        {
            ItemImageBGs[i].color = Color.white;
            ItemImages[i].texture = Items.ElementAt(Inventory[i].ItemID).Value.SmallSprite;
        }

        BaseItem SelectedItem = GetSelectedItemObject();
        ItemNameText.text = $"{SelectedItem.Name}";
        ItemNameText.color = SelectedItem.NameColor;

        if (SelectedItem.Uses > 1)
        {
            ItemNameText.text += $" ({SelectedItem.Uses})";
        }

        ItemImageBGs[ItemSelection].color = SelectionColor;
    }
    #endregion

    #region Function Handling
    public BaseItem GetItem(string name)
    {
        if (Items.ContainsKey(name))
        {
            return Items[name];
        }

        return null;
    }

    public BaseItem GetItem(int id) => GetItem(Items.ElementAt(id).Value.Name);

    public void AddItem(BaseItem item)
    {
        if (item != null && !Items.ContainsKey(item.name))
        {
            Items.Add(item.name, item);
            return;
        }

        Debug.LogWarning("Attempted to add an item that was either null or was already apart of the items dictionary");
    }

    public void RemoveItem(string name)
    {
        if (Items.ContainsKey(name))
        {
            Items.Remove(name);
            return;
        }

        Debug.LogWarning("Attempted to remove an item that wasn't apart of the items dictionary");
    }

    public void RemoveItem(BaseItem item) => RemoveItem(item.name);
    
    public int GetSelectedItem() => Inventory[ItemSelection].ItemID;

    public bool IsInventoryFull() => Inventory.All(i => i.ItemID != 0);

    public bool HasNoItems() => Inventory.All(i => i.ItemID == 0);

    public BaseItem GetSelectedItemObject()
    {
        if (Inventory[ItemSelection].ItemID != 0 && Inventory[ItemSelection].ItemInstance == null)
        {
            CreateItemInstance();
            return Inventory[ItemSelection].ItemInstance.GetComponent<BaseItem>();
        }

        return Inventory[ItemSelection].ItemInstance != null ? Inventory[ItemSelection].ItemInstance : GetItem(GetSelectedItem());
    }
    #endregion

    #region Item Instances & Collection
    private void CreateItemInstance(int? at = null)
    {
        int index = at ?? ItemSelection;
        if (Inventory[index].ItemID == 0)
        {
            return;
        }
        if (Inventory[index].ItemInstance == null)
        {
            BaseItem itemobj = GetItem(Inventory[index].ItemID);
            GameObject NewInstance = Instantiate(itemobj.gameObject, transform);
            NewInstance.name = itemobj.gameObject.name;
            Inventory[index].ItemInstance = NewInstance.GetComponent<BaseItem>();
        }
    }

    public void CollectItem(int ItemID, BaseItem instance = null)
    {
        if (GetSelectedItem() == 0)
        {
            SetItem(ItemSelection, ItemID, instance);
            UpdateItemUI();
            return;
        }

        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i].ItemID == 0)
            {
                SetItem(i, ItemID, instance);
                UpdateItemUI();
                return;
            }
        }

        SetItem(ItemSelection, ItemID, instance);
        UpdateItemUI();
    }

    public void ReplaceCurrentItem(int ItemID)
    {
        if (Inventory[ItemSelection].ItemInstance != null)
        {
            Destroy(Inventory[ItemSelection].ItemInstance.gameObject);
        }

        SetItem(ItemSelection, ItemID);
        UpdateItemUI();
    }

    public void DropItem(int index)
    {
        var item = Inventory[index];
        if (item.ItemID == 0 || item.ItemInstance == null)
        {
            return;
        }

        BaseItem itemToDrop = item.ItemInstance;
        Vector3 spawnPosition = GameControllerScript.Instance.player.ItemDropLocation.transform.position;

        GameObject droppedItem = new GameObject($"Pickup_{itemToDrop.Name}")
        {
            transform = { position = spawnPosition },
            tag = "Item"
        };

        var pickup = droppedItem.AddComponent<PickupScript>();
        pickup.DroppedItem = true;

        typeof(PickupScript).GetField("ID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(pickup, Inventory[index].ItemID);
        pickup.GetType().GetField("PresentMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(pickup, false);

        var collider = droppedItem.AddComponent<CapsuleCollider>();
        collider.isTrigger = true;
        collider.center = new Vector3(0, 1, 0);
        collider.radius = 1.5f;
        collider.height = 2f;

        GameObject spriteObject = new GameObject("Sprite")
        {
            transform = { parent = droppedItem.transform, localPosition = Vector3.zero, localScale = new Vector3(2f, 2f, 2f) }
        };

        SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        if (itemToDrop.BigSprite is Texture2D texture)
        {
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
        }
        else
        {
            Debug.LogWarning("BigSprite is not a Texture2D, cannot create Sprite.");
        }

        spriteRenderer.material = GameControllerScript.Instance.SpriteRenderer;
        spriteObject.AddComponent<Billboard>();
        spriteObject.AddComponent<PickupAnimationScript>();
        spriteObject.AddComponent<SpriteColorManager>();

        itemToDrop.transform.SetParent(droppedItem.transform);
        itemToDrop.gameObject.SetActive(true);

        ClearItem(index);
        UpdateItemUI();
    }
    #endregion

    #region Change References void
    public void ChangeReferences(List<RawImage> itemImages, List<Image> itemImgBgs)
    {
        ItemImages = itemImages;
        ItemImageBGs = itemImgBgs;
    }
    #endregion

    #region Nested Types
    [Serializable]
    public struct HeldItem
    {
        public int ItemID;
        public BaseItem ItemInstance;
    }
    private enum ExecutionType { Use, Pickup, Select, Deselect }
    #endregion

    #region Fields & Serialized
    private Dictionary<string, BaseItem> Items = new Dictionary<string, BaseItem>();
    public HeldItem[] Inventory;
    private KeyCode[] KeyIndex = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };

    [Header("UI References")]
    [SerializeField] private List<RawImage> ItemImages = new List<RawImage>();
    [SerializeField] private List<Image> ItemImageBGs = new List<Image>();
    [SerializeField] private TextMeshProUGUI ItemNameText;
    [SerializeField] private Color SelectionColor = Color.red;
    [HideInInspector] public int ItemSelection = 0;
    public static ItemManager Instance;
    #endregion 
}