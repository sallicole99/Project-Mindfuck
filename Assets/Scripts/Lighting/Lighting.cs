using UnityEngine;
using System.Collections.Generic;

public class Lighting : MonoBehaviour
{
    public bool IsExcluded(GameObject obj)
    {
        if (excludedObjects.Contains(obj))
        {
            return true;
        }

        Transform parent = obj.transform;
        while (parent != null)
        {
            if (excludedParentObjects.Contains(parent.gameObject))
            {
                return true;
            }
            parent = parent.parent;
        }

        return false;
    }

    public bool isChildExcluded(GameObject obj)
    {
        if (excludedObjects.Contains(obj))
        {
            return true;
        }

        return false;
    }

    public Color GetColorAtPosition(Vector3 position)
    {
        float distance = Vector3.Distance(transform.position, position);
        if (distance <= minPower)
        {
            Color initialColor = shadowColor * intensity;
            return initialColor;
        }
        else if (distance <= maxPower)
        {
            float lerpValue = 1f - ((distance - minPower) / (maxPower - minPower));
            return Color.Lerp(Color.white, shadowColor, lerpValue) * intensity;
        }
        else
        {
            return Color.white;
        }
    }

    public float CalculateInfluence(Vector3 position)
    {
        float distance = Vector3.Distance(transform.position, position);
        if (distance <= minPower)
        {
            return 1f;
        }
        else if (distance <= maxPower)
        {
            return 1f - ((distance - minPower) / (maxPower - minPower));
        }
        else
        {
            return 0f;
        }
    }

    [Header("Light Settings")]
    [SerializeField] private Color shadowColor = new Color(0.47f, 0.47f, 0.47f, 1f);
    [SerializeField] private float minPower = 3f, maxPower = 5f, intensity = 1.0f;

    private List<GameObject> excludedObjects = new List<GameObject>(), excludedParentObjects = new List<GameObject>();
}