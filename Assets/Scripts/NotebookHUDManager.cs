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

    private int collectedCount = 0;
    private int totalSlots;
    private List<Image> slots = new List<Image>();

    void Awake()
    {
        // 19 notebooks → 10 slots (ceil of 19/2)
        totalSlots = Mathf.CeilToInt(totalNotebooks / 2f);
        BuildIcons();
    }

    void BuildIcons()
    {
        slots.Clear();

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject obj = Instantiate(iconPrefab, iconContainer);
            Image img = obj.GetComponent<Image>();
            img.sprite = notebookEmpty;
            slots.Add(img);
        }
    }

    // Your Notebook Problems collection
    public void OnNotebookCollected()
    {
        collectedCount++;
        collectedCount = Mathf.Clamp(collectedCount, 0, totalNotebooks);
        RefreshIcons();

        // Bounce the changed slot
        int changedSlot = Mathf.CeilToInt(collectedCount / 2f) - 1;
        if (changedSlot >= 0 && changedSlot < slots.Count)
        {
            RectTransform rt = slots[changedSlot].GetComponent<RectTransform>();
            StartCoroutine(BounceIcon(rt));
        }

        // All collected!
        if (collectedCount >= totalNotebooks)
        {
            StartCoroutine(AllCollectedSequence());
        }
    }

    void RefreshIcons()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            // How many notebooks in this slot rea
            int fullThreshold  = (i + 1) * 2;   // needs 2 notebooks to be full
            int halfThreshold  = i * 2 + 1;      // needs 1 notebook to be half

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

        // Scale up
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 12f;
            icon.localScale = Vector3.Lerp(originalScale, bigScale, t);
            yield return null;
        }

        // Scale back down
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            icon.localScale = Vector3.Lerp(bigScale, originalScale, t);
            yield return null;
        }

        icon.localScale = originalScale;
    }

    // Call this when all notebooks are collected
    private IEnumerator AllCollectedSequence()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(SlideUp());
    }

    private IEnumerator SlideUp()
    {
        RectTransform container = iconContainer.GetComponent<RectTransform>();
        Vector2 startPos = container.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, 300f); // slides 300 units up

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f; // speed of slide, lower = slower
            // EaseIn curve so it accelerates as it leaves
            container.anchoredPosition = Vector2.Lerp(startPos, endPos, t * t);
            yield return null;
        }
    }
}