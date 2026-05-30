using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdditionalGameCustomizer : MonoBehaviour
{
    #region UnityCallbacks
    private void Awake() => Instance = this;
    
    private void Start()
    {
        InitializeCustomAdditions();
        SkyBoxHandling();
        ScrambleItems();
        ItemSlotAmout();
    }

    private void Update()
    {
        CameraShaking();
        FlashlightCode();
        StaminaStyleHandling();
        KeyFunctions();
        CurrencySystem();
        HandleInventoryVisibility();
        HandleLeftHandSlot();
    }
    #endregion

    #region Initialization
    private void InitializeCustomAdditions()
    {
        TMP.SetActive(OldDetentionTimer);
        Clock.SetActive(!OldDetentionTimer);
        GaugeManager.SetActive(Gauges);
    }
    #endregion

    #region VisualEffects
    private void CameraShaking()
    {
        GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        if (cameraObject != null && CameraShake)
        {
            Camera cameraComponent = cameraObject.GetComponent<Camera>();
            if (cameraComponent != null)
                cameraComponent.fieldOfView = Random.Range(58, 62);
        }
    }

    private void FlashlightCode()
    {
        GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        if (cameraObject != null)
        {
            Light light = cameraObject.GetComponent<Light>();
            if (light != null)
                light.enabled = isFlashlightOn;
        }
    }
    #endregion

    #region StaminaManagement
    private void StaminaStyleHandling()
    {
        var staminaMap = new Dictionary<StaminaDisplay, GameObject>
        {
            { StaminaDisplay.Old,      OldStamina      },
            { StaminaDisplay.PreOld,   PreOldStamina   },
            { StaminaDisplay.Normal,   NewStamina      },
            { StaminaDisplay.Vertical, VerticalStamina },
            { StaminaDisplay.Circle,   CircleStamina   }
        };

        OldStamina.SetActive(false);
        PreOldStamina.SetActive(false);
        NewStamina.SetActive(false);
        VerticalStamina.SetActive(false);
        CircleStamina.SetActive(false);

        if (staminaMap.ContainsKey(StaminaStyle))
            staminaMap[StaminaStyle].SetActive(true);

        if (StaminaStyle == StaminaDisplay.Old)
        {
            bool YouNeedRest = GameControllerScript.Instance.player.stamina < 0f;
            if (warning.activeSelf != YouNeedRest)
                warning.SetActive(YouNeedRest);
        }
    }
    #endregion

    #region InputHandling
    private void KeyFunctions()
    {
        if (Time.timeScale == 0f) return;

        if (ItemDropping)
        {
            int selectedSlot = ItemManager.Instance.ItemSelection;
            var slot = ItemManager.Instance.Inventory[selectedSlot];

            // Shift + R — drop bulk amount
            if (Input.GetKeyDown(dropItemButton) && Input.GetKey(KeyCode.LeftShift))
            {
                if (slot.ItemID != 0)
                    ItemManager.Instance.DropBulkFromStack(selectedSlot, bulkDropAmount);
            }
            else if (Input.GetKeyDown(dropItemButton))
            {
                if (slot.ItemID != 0)
                    ItemManager.Instance.DropOneFromStack(selectedSlot);
            }
        }

        // Flashlight on C key
        if (FlashLight && Input.GetKeyDown(KeyCode.C))
            isFlashlightOn = !isFlashlightOn;
    }
    #endregion

    #region SkyboxManagement
    private void SkyBoxHandling()
    {
        switch (SetSkybox)
        {
            case SkyboxStyle.Default: RenderSettings.skybox = DefaultSky;  currentSkybox = SkyboxStyle.Default; break;
            case SkyboxStyle.Day:     RenderSettings.skybox = NormalSky;   currentSkybox = SkyboxStyle.Day;     break;
            case SkyboxStyle.Sunset:  RenderSettings.skybox = TwilightSky; currentSkybox = SkyboxStyle.Sunset;  break;
            case SkyboxStyle.Night:   RenderSettings.skybox = NightSky;    currentSkybox = SkyboxStyle.Night;   break;
        }
    }
    #endregion

    #region Inventory Visibility
    private void HandleInventoryVisibility()
    {
        if (slotsAmount != SlotsAmount._9 || _9_Slots == null) return;
        _9_Slots.SetActive(true);
    }

    private void HandleLeftHandSlot()
    {
        if (leftHandSlot == null) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (leftHandSlotActive)
            {
                // ── Return item directly to the slot it came from ──────────
                leftHandSlotActive = false;
                leftHandSlot.SetActive(false);

                if (leftHandInventory.ItemID != 0)
                {
                    var inv = ItemManager.Instance.Inventory;

                    // Try to put it back in the original slot first
                    if (leftHandSourceSlot >= 0 && leftHandSourceSlot < inv.Length)
                    {
                        var src = inv[leftHandSourceSlot];

                        if (src.ItemID == 0)
                        {
                            // Slot is now empty — restore directly
                            inv[leftHandSourceSlot].ItemID       = leftHandInventory.ItemID;
                            inv[leftHandSourceSlot].ItemInstance = leftHandInventory.ItemInstance;
                            inv[leftHandSourceSlot].StackCount   = leftHandStackCount;
                        }
                        else if (src.ItemID == leftHandInventory.ItemID && src.StackCount < src.MaxStack)
                        {
                            // Same item and has space — merge back in
                            int space = src.MaxStack - src.StackCount;
                            int add   = Mathf.Min(leftHandStackCount, space);
                            inv[leftHandSourceSlot].StackCount += add;
                            int leftover = leftHandStackCount - add;

                            // Any leftover spills to the next available empty slot
                            if (leftover > 0)
                                SpillToEmptySlot(leftHandInventory.ItemID, leftHandInventory.ItemInstance, leftover);
                        }
                        else
                        {
                            // Source slot is occupied by a different item — find any empty slot
                            SpillToEmptySlot(leftHandInventory.ItemID, leftHandInventory.ItemInstance, leftHandStackCount);
                        }
                    }

                    leftHandInventory.ItemID       = 0;
                    leftHandInventory.ItemInstance = null;
                    leftHandStackCount             = 0;
                    leftHandSourceSlot             = -1;
                    ItemManager.Instance.UpdateItemUI();
                }
            }
            else
            {
                // ── Take 1 from selected slot into left hand ───────────────
                int selectedSlot = ItemManager.Instance.ItemSelection;
                int selectedID   = ItemManager.Instance.GetSelectedItem();

                if (selectedID != 0 && ItemManager.Instance.Inventory[selectedSlot].StackCount > 0)
                {
                    leftHandInventory.ItemID       = selectedID;
                    leftHandInventory.ItemInstance = ItemManager.Instance.GetSelectedItemObject();
                    leftHandStackCount             = 1;
                    leftHandSourceSlot             = selectedSlot; // remember where it came from

                    // Remove 1 from the source slot
                    ItemManager.Instance.Inventory[selectedSlot].StackCount--;
                    if (ItemManager.Instance.Inventory[selectedSlot].StackCount <= 0)
                        ItemManager.Instance.ClearItem(selectedSlot);

                    ItemManager.Instance.UpdateItemUI();
                    leftHandSlotActive = true;
                    leftHandSlot.SetActive(true);
                }
            }
        }

        // ── Use left hand item with Q when slot is active ─────────────────
        if (leftHandSlotActive && leftHandInventory.ItemID != 0)
        {
            if (Singleton<InputManager>.Instance.GetActionKey(InputAction.UseItem))
            {
                BaseItem item = leftHandInventory.ItemInstance;
                if (item != null)
                {
                    bool shouldDestroy = item.OnUse();
                    if (shouldDestroy)
                    {
                        item.Uses--;
                        if (item.Uses <= 0)
                        {
                            item.OnDeselect();
                            Destroy(item.gameObject);
                            leftHandInventory.ItemID       = 0;
                            leftHandInventory.ItemInstance = null;
                            leftHandStackCount             = 0;
                            leftHandSourceSlot             = -1;
                            leftHandSlotActive             = false;
                            leftHandSlot.SetActive(false);
                        }
                    }
                }
            }
        }

        // ── Update icon ───────────────────────────────────────────────────
        if (leftHandSlotActive && leftHandItemImage != null && leftHandInventory.ItemID != 0)
            leftHandItemImage.texture = ItemManager.Instance.GetItem(leftHandInventory.ItemID)?.SmallSprite;
    }

    // Puts an item into the first available empty inventory slot,
    // or merges into an existing stack if possible,
    // or drops it in the world if truly nowhere to put it.
    private void SpillToEmptySlot(int itemID, BaseItem instance, int stackCount)
    {
        var inv       = ItemManager.Instance.Inventory;
        int remaining = stackCount;

        // Pass 1: try to top up existing stacks of the same item
        for (int i = 0; i < inv.Length && remaining > 0; i++)
        {
            if (inv[i].ItemID != itemID) continue;
            if (inv[i].StackCount >= inv[i].MaxStack) continue;

            int space = inv[i].MaxStack - inv[i].StackCount;
            int add   = Mathf.Min(remaining, space);
            inv[i].StackCount += add;
            remaining -= add;
        }

        // Pass 2: spill remainder into empty slots
        for (int i = 0; i < inv.Length && remaining > 0; i++)
        {
            if (inv[i].ItemID != 0) continue;

            int slotMax = inv[i].MaxStack > 0 ? inv[i].MaxStack : 1;
            int add     = Mathf.Min(remaining, slotMax);
            inv[i].ItemID       = itemID;
            inv[i].ItemInstance = instance;
            inv[i].StackCount   = add;
            remaining -= add;
        }

        // Still leftover — inventory is completely full, drop in the world
        if (remaining > 0)
        {
            Debug.LogWarning($"Left hand return: no space for {remaining} of item {itemID}, dropping in world.");
            SpawnLeftoverPickup(itemID, instance, remaining);
        }
    }

    // Spawns a world pickup at the player's position for items that couldn't fit in inventory
    private void SpawnLeftoverPickup(int itemID, BaseItem instance, int amount)
    {
        BaseItem itemRef = ItemManager.Instance.GetItem(itemID);
        if (itemRef == null) return;

        Vector3 spawnPos = GameControllerScript.Instance.player.ItemDropLocation.transform.position;

        GameObject dropped = new GameObject($"Pickup_{itemRef.Name}");
        dropped.transform.position = spawnPos;
        dropped.tag = "Item";

        var pickup = dropped.AddComponent<PickupScript>();
        pickup.DroppedItem  = true;
        pickup.stackAmount  = amount;
        pickup.maxStack     = amount;

        typeof(PickupScript)
            .GetField("ID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(pickup, itemID);
        typeof(PickupScript)
            .GetField("PresentMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(pickup, false);

        var col = dropped.AddComponent<CapsuleCollider>();
        col.isTrigger = true;
        col.center    = new Vector3(0, 1, 0);
        col.radius    = 1.5f;
        col.height    = 2f;

        GameObject spriteObj = new GameObject("Sprite");
        spriteObj.transform.parent        = dropped.transform;
        spriteObj.transform.localPosition = Vector3.zero;
        spriteObj.transform.localScale    = new Vector3(2f, 2f, 2f);

        var sr = spriteObj.AddComponent<SpriteRenderer>();
        if (itemRef.BigSprite is Texture2D tex)
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);

        sr.material = GameControllerScript.Instance.SpriteRenderer;
        spriteObj.AddComponent<Billboard>();
        spriteObj.AddComponent<PickupAnimationScript>();
        spriteObj.AddComponent<SpriteColorManager>();
    }
    #endregion

    #region Item Customization
    private void CurrencySystem()
    {
        if (ReworkedCurrency)
        {
            Counter.SetActive(true);
            AudioSource audioDevice = GameControllerScript.Instance.audioDevice;
            currencyCounterBG.text = "$" + Cash.ToString("F2");
            currencyCounter.text   = currencyCounterBG.text;

            if (Cash >= 0.25)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (Sych.ScreenRaycastMatchesTag("VendingMachine", out RaycastHit hit, 10f))
                    {
                        var vendingMachine = hit.collider.GetComponent<VendingMachineScript>();
                        if (vendingMachine != null && !ItemManager.Instance.IsInventoryFull())
                        {
                            Cash -= 0.25;
                            audioDevice.PlayOneShot(aud_Drop);
                            vendingMachine.DispenseItem();
                        }
                    }
                    else if (Sych.ScreenRaycastMatchesTag("Phone", out hit, 10f))
                    {
                        var tapePlayer = hit.collider.GetComponent<TapePlayerScript>();
                        if (tapePlayer != null)
                        {
                            Cash -= 0.25;
                            audioDevice.PlayOneShot(aud_Drop);
                            tapePlayer.Play();
                        }
                    }
                }
            }
        }
        else
        {
            Counter.SetActive(false);
        }
    }

    private void ScrambleItems()
    {
        if (RandomizeItems)
        {
            List<Vector3> list = new List<Vector3>();
            foreach (PickupScript pickupScript in FindObjectsOfType<PickupScript>())
                if (pickupScript.gameObject != quarter && !pickupScript.SpawnAtRandom)
                    list.Add(pickupScript.transform.position);

            foreach (PickupScript pickupScript2 in FindObjectsOfType<PickupScript>())
            {
                if (pickupScript2.gameObject != quarter && !pickupScript2.SpawnAtRandom)
                {
                    int index = Random.Range(0, list.Count);
                    pickupScript2.transform.position = list[index];
                    list.RemoveAt(index);
                }
            }
        }
    }

    private void ItemSlotAmout()
    {
        if (slotsAmount == SlotsAmount._5)
        {
            ItemManager.Instance.Inventory = _5_Slots_Inventory;
            ItemManager.Instance.ChangeReferences(_5slotsItemImage, _5slotsBG);
            _3_Slots.SetActive(false);
            _5_Slots.SetActive(false);
            _9_Slots?.SetActive(false);
            RectTransform counterRect = Counter.GetComponent<RectTransform>();
            counterRect.anchoredPosition = new Vector2(15f, counterRect.anchoredPosition.y);
            _5_Slots.SetActive(true);
        }
        else if (slotsAmount == SlotsAmount._9)
        {
            ItemManager.Instance.Inventory = _9_Slots_Inventory;
            ItemManager.Instance.ChangeReferences(_9slotsItemImage, _9slotsBG);
            _3_Slots.SetActive(false);
            _5_Slots.SetActive(false);
            _9_Slots?.SetActive(true);
        }
        else
        {
            _3_Slots.SetActive(true);
            _5_Slots.SetActive(false);
            _9_Slots?.SetActive(false);
        }
    }
    #endregion

    #region SerializedFields
    [Header("Gameplay Addons")]
    public bool RandomizeJumps;
    public bool NoYCTP, DetentionAfterScissorUse, AnOldRule, ItemDropping, SkipCraftersAttack, ReworkedCurrency, RandomizeItems, DragToDetention;
    [SerializeField] private KeyCode dropItemButton = KeyCode.R;
    [SerializeField] private int bulkDropAmount = 32;
    [SerializeField] private SlotsAmount slotsAmount = SlotsAmount._9;

    [Header("Visual Addons")]
    public StaminaDisplay StaminaStyle = StaminaDisplay.Normal;
    public bool RandomizeBookColor, Indicator = true, FinalModeTV = true, Gauges = true, OldDetentionTimer, ExitCounter, FlashLight, CameraShake, FreeRoamCamera;
    public SkyboxStyle SetSkybox = SkyboxStyle.Day;

    [Header("Serialized References")]
    public Sprite[] BookColors;
    public Material NormalSky, NormalRedSky, NightSky, RedNightSky, TwilightSky, RedTwilightSky, DefaultSky;
    [SerializeField] private GameObject warning, Clock, TMP, OldStamina, PreOldStamina, NewStamina, VerticalStamina, CircleStamina, GaugeManager, Counter, quarter, _3_Slots, _5_Slots;
    [SerializeField] private GameObject _9_Slots;
    [SerializeField] private GameObject leftHandSlot;
    [SerializeField] private RawImage leftHandItemImage;
    [SerializeField] private TMP_Text currencyCounter, currencyCounterBG;
    [SerializeField] private AudioClip aud_Drop;
    [SerializeField] private ItemManager.HeldItem[] _5_Slots_Inventory;
    [SerializeField] private ItemManager.HeldItem[] _9_Slots_Inventory;
    [SerializeField] private List<RawImage> _5slotsItemImage  = new List<RawImage>();
    [SerializeField] private List<Image>    _5slotsBG         = new List<Image>();
    [SerializeField] private List<RawImage> _9slotsItemImage  = new List<RawImage>();
    [SerializeField] private List<Image>    _9slotsBG         = new List<Image>();
    #endregion

    #region RuntimeVariables
    private bool isFlashlightOn     = false;
    private bool leftHandSlotActive = false;
    private int  leftHandStackCount = 0;
    private int  leftHandSourceSlot = -1;   // tracks which slot the left hand item came from
    private ItemManager.HeldItem leftHandInventory;
    public static AdditionalGameCustomizer Instance;
    [HideInInspector] public SkyboxStyle currentSkybox;
    [HideInInspector] public double Cash = 0.00;
    #endregion

    #region Enums
    public enum SlotsAmount    { _3, _5, _9 }
    public enum SkyboxStyle    { Default, Day, Sunset, Night }
    public enum StaminaDisplay { Old, PreOld, Normal, Vertical, Circle }
    #endregion
}