using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
public class NotebookHUDManager : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite notebookEmpty;
    public Sprite notebookHalf;
    public Sprite notebookFull;
 
    [Header("Layout")]
    public Transform iconContainer;   // The HorizontalLayoutGroup parent
    public GameObject iconPrefab;     // A simple UI Image prefab ig
 
    [Header("Settings")]
    public int totalNotebooks = 19;
 
    [Header("References")]
    public ExitHUDManager exitHUD; // drag ExitHUDManager GameObject here in Inspector
 
    private int collectedCount = 0;
    private int totalSlots;
    private List<Image> slots = new List<Image>();
    private List<Image> overlayImages = new List<Image>();
 
    void Awake()
    {
        totalSlots = Mathf.CeilToInt(totalNotebooks / 2f);
        BuildIcons();
    }
 
    void BuildIcons()
    {
        slots.Clear();
        overlayImages.Clear();
 
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject obj = Instantiate(iconPrefab, iconContainer);
            Image img = obj.GetComponent<Image>();
            img.sprite = notebookEmpty;
            slots.Add(img);
 
            // Add a red flash overlay on top of each icon
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
 
    public void OnNotebookCollected()
    {
        collectedCount++;
        collectedCount = Mathf.Clamp(collectedCount, 0, totalNotebooks);
        RefreshIcons();
 
        int changedSlot = Mathf.CeilToInt(collectedCount / 2f) - 1;
        if (changedSlot >= 0 && changedSlot < slots.Count)
        {
            RectTransform rt = slots[changedSlot].GetComponent<RectTransform>();
            StartCoroutine(BounceIcon(rt));
        }
 
        if (collectedCount >= totalNotebooks)
        {
            StartCoroutine(AllCollectedSequence());
        }
    }
 
    void RefreshIcons()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int fullThreshold  = (i + 1) * 2;
            int halfThreshold  = i * 2 + 1;
 
            if (collectedCount >= fullThreshold)
                slots[i].sprite = notebookFull;
            else if (collectedCount >= halfThreshold)
                slots[i].sprite = notebookHalf;
            else
                slots[i].sprite = notebookEmpty;
        }
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
 
    private IEnumerator AllCollectedSequence()
{
    yield return new WaitForSeconds(1f);
    yield return StartCoroutine(SlideUp());

    // Find it via code instead of Inspector reference
    ExitHUDManager exit = FindObjectOfType<ExitHUDManager>();
    Debug.Log("ExitHUDManager found: " + (exit != null));
    if (exit != null)
        exit.SlideIn();
}

    private IEnumerator SlideUp()
    {
        RectTransform container = iconContainer.GetComponent<RectTransform>();
        Vector2 startPos = container.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, 300f);
 
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            container.anchoredPosition = Vector2.Lerp(startPos, endPos, t * t);
            yield return null;
        }
    }
}
