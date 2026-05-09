using UnityEngine;

public class BaseItem : MonoBehaviour
{
    #region Virtual Hooks
    public virtual bool OnUse() => true;
    public virtual void OnSelect() { }
    public virtual void OnDeselect() { }
    public virtual void OnPickup() { }

    public virtual BaseItem CreateInstance() => Instantiate(this);
    #endregion

    #region Helpers
    protected bool SendRay(string tag, out RaycastHit rayHit, float range = 10f)
    {
        rayHit = default;

        if (Sych.ScreenCenterRaycast(out RaycastHit hit))
        {
            bool withinRange = hit.transform.IsWithinDistance(range);
            bool tagMatch = string.IsNullOrEmpty(tag) || hit.collider.CompareTag(tag);

            if (withinRange && tagMatch)
            {
                rayHit = hit;
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Serialized Data
    [Tooltip("The name of the item")] public string Name;
    [Tooltip("The color of the item name")] public Color NameColor = Color.black;

    [Header("Sprite")]
    [Header("Sprite")]
    [Tooltip("Sprite of the item used for pickups")] public Texture BigSprite;
    [Tooltip("Sprite of the item used in the HUD")] public Texture SmallSprite;

    [Header("Settings")]
    [Tooltip("How many uses the item has")] public int Uses = 1;
    #endregion
}