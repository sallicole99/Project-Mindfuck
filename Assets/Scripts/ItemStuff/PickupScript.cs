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
            Sprite itemSprite = Sprite.Create((Texture2D)itemTexture, new Rect(0, 0, itemTexture.width, itemTexture.height), new Vector2(0.5f, 0.5f), 100);
            cachedSprites.Add(ID, itemSprite);
        }

        GetComponentInChildren<SpriteRenderer>().sprite = cachedSprites[ID];
        gameObject.name = $"Pickup_{ItemManager.Instance.GetItem(ID).Name}";
    }

    public override void Interact()
    {
        GameControllerScript.Instance.audioDevice.PlayOneShot(GameControllerScript.Instance.aud_ItemCollect);
        if (AdditionalGameCustomizer.Instance?.ReworkedCurrency == true && ID == 5)
        {
            AdditionalGameCustomizer.Instance.Cash += 0.25f;
            transform.gameObject.SetActive(false);
            return;
        }
        else if (SlotStuffs(true))
        {
            if (!DroppedItem)
            {
                transform.gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }

            ItemManager.Instance.CollectItem(ID, GetHeldInstance());
            return;
        }

        int orgID = ID;
        BaseItem orgItem = GetHeldInstance();

        ID = ItemManager.Instance.GetSelectedItem();
        BaseItem newItem = ItemManager.Instance.GetSelectedItemObject();
        newItem.transform.parent = transform;

        UpdateSpriteAndID();

        if (SlotStuffs(false))
        {
            transform.gameObject.SetActive(true);
        }

        ItemManager.Instance.CollectItem(orgID, orgItem);
    }
    #endregion

    #region Utility Methods
    private BaseItem GetHeldInstance() => GetComponentInChildren<BaseItem>();

    public bool SlotStuffs(bool trueOrNot)
    {
        for (int i = 0; i < ItemManager.Instance.Inventory.Length; i++)
        {
            if (ItemManager.Instance.Inventory[i].ItemID == 0)
            {
                return trueOrNot;
            }
        }
        return !trueOrNot;
    }
    #endregion

    #region Configuration & State
    [Header("Pickup Settings")]
    [SerializeField] private int ID;
    [SerializeField] private bool PresentMode;
    public bool SpawnAtRandom;

    private static Dictionary<int, Sprite> cachedSprites = new Dictionary<int, Sprite>();
    [HideInInspector] public bool DroppedItem;
    private AILocationSelectorScript wanderer;
    private Transform location;
    #endregion
}