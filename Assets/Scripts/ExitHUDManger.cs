using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
public class ExitHUDManager : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite exitEmpty;
    public Sprite exitHalf;
    public Sprite exitFull;
 
    [Header("Layout")]
    public Transform iconContainer;   // The HorizontalLayoutGroup parent
    public GameObject iconPrefab;     // A simple UI Image prefab
 
    [Header("Settings")]
    public int totalExits = 4;
 
    private int usedCount = 0;
    private int totalSlots;
    private List<Image> slots = new List<Image>();
    private List<Image> overlayImages = new List<Image>();
 
    private Vector2 hiddenPos;
    private Vector2 shownPos;
 
    void Awake()
    {
        totalSlots = Mathf.CeilToInt(totalExits / 2f);
        BuildIcons();
    }
 
    void Start()
    {
        RectTransform container = iconContainer.GetComponent<RectTransform>();
 
        // Save the target (shown) position — wherever you placed it in the Canvas
        shownPos = container.anchoredPosition;
 
        // Hide it above the screen by offsetting upward by 300 units
        hiddenPos = shownPos + new Vector2(0, 300f);
        container.anchoredPosition = hiddenPos;
    }
 
    void BuildIcons()
    {
        slots.Clear();
        overlayImages.Clear();
 
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject obj = Instantiate(iconPrefab, iconContainer);
            Image img = obj.GetComponent<Image>();
            img.sprite = exitEmpty;
            slots.Add(img);
 
            // Add a flash overlay on top of each icon
            GameObject overlay = new GameObject("FlashOverlay");
            overlay.transform.SetParent(obj.transform, false);
            RectTransform rt = overlay.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(1f, 0f, 0f, 0f);
            overlayImg.raycastTarget = false;
            overlayImages.Add(overlayImg);
        }
    }
 
    // Called by NotebookHUDManager after it finishes sliding up
    public void SlideIn()
    {
        StartCoroutine(SlideDown());
    }
 
    // Called when the player goes through an exit
    public void OnExitUsed()
    {
        usedCount++;
        usedCount = Mathf.Clamp(usedCount, 0, totalExits);
        RefreshIcons();
 
        int changedSlot = Mathf.CeilToInt(usedCount / 2f) - 1;
        if (changedSlot >= 0 && changedSlot < slots.Count)
        {
            RectTransform rt = slots[changedSlot].GetComponent<RectTransform>();
            StartCoroutine(BounceIcon(rt));
        }
    }
 
    void RefreshIcons()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int fullThreshold = (i + 1) * 2;
            int halfThreshold = i * 2 + 1;
 
            if (usedCount >= fullThreshold)
                slots[i].sprite = exitFull;
            else if (usedCount >= halfThreshold)
                slots[i].sprite = exitHalf;
            else
                slots[i].sprite = exitEmpty;
        }
    }
 
    private IEnumerator SlideDown()
    {
        RectTransform container = iconContainer.GetComponent<RectTransform>();
 
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f; // speed of slide in, lower = slower
            // EaseOut curve so it decelerates into position
            float eased = 1f - (1f - t) * (1f - t);
            container.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, eased);
            yield return null;
        }
 
        container.anchoredPosition = shownPos;
    }
 
    private IEnumerator BounceIcon(RectTransform icon)
    {
        Vector3 originalScale = icon.localScale;
        Vector3 bigScale = originalScale * 1.4f;
 
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 12f;
            icon.localScale = Vector3.Lerp(originalScale, bigScale, t);
            yield return null;
        }
 
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            icon.localScale = Vector3.Lerp(bigScale, originalScale, t);
            yield return null;
        }
 
        icon.localScale = originalScale;
    }
}