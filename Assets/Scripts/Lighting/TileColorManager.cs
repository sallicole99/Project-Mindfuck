using UnityEngine;
using System.Collections.Generic;

public class TileColorManager : MonoBehaviour
{
    private void Start()
    {
        InitializeRenderers();

        if (AllLighting.Instance.Lights.Count > 0)
        {
            BlendLightingColors();
        }
    }

    private void OnDestroy() => CleanupRenderers();

    private void InitializeRenderers()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            RendererMaterials rendererMaterials;
            rendererMaterials.renderer = renderer;
            rendererMaterials.isExcluded = false;

            foreach (Lighting currentLighting in AllLighting.Instance.Lights)
            {
                if (currentLighting.isChildExcluded(renderer.gameObject))
                {
                    rendererMaterials.isExcluded = true;
                    break;
                }
            }

            rendererMaterials.originalMaterials = renderer.sharedMaterials;
            rendererMaterials.originalColors = new Dictionary<int, Dictionary<string, Color>>();
            rendererMaterials.blocks = new MaterialPropertyBlock[rendererMaterials.originalMaterials.Length];

            for (int i = 0; i < rendererMaterials.originalMaterials.Length; i++)
            {
                Material mat = rendererMaterials.originalMaterials[i];
                var colorDict = new Dictionary<string, Color>();

                foreach (string prop in ColorProperties)
                {
                    if (mat.HasProperty(prop))
                    {
                        colorDict[prop] = mat.GetColor(prop);
                    }
                }

                rendererMaterials.originalColors[i] = colorDict;

                rendererMaterials.blocks[i] = new MaterialPropertyBlock();
                rendererMaterials.renderer.GetPropertyBlock(rendererMaterials.blocks[i], i);
            }

            rendererMaterialsList.Add(rendererMaterials);
        }
    }

    private void CleanupRenderers()
    {
        foreach (RendererMaterials rendererMaterials in rendererMaterialsList)
        {
            for (int i = 0; i < rendererMaterials.blocks.Length; i++)
            {
                rendererMaterials.renderer.SetPropertyBlock(null, i);
            }
        }
        rendererMaterialsList.Clear();
    }

    public Color GetTileColor()
    {
        return CalculateFinalColor();
    }

    private void BlendLightingColors()
    {
        Color finalColor = CalculateFinalColor();
        ApplyColor(finalColor);
    }

    private Color CalculateFinalColor()
    {
        Color finalColor = Color.clear;
        float totalInfluence = 0f;

        foreach (Lighting currentLighting in AllLighting.Instance.Lights)
        {
            if (!currentLighting.IsExcluded(gameObject))
            {
                float influence = currentLighting.CalculateInfluence(transform.position);
                if (influence > 0)
                {
                    Color lightColor = currentLighting.GetColorAtPosition(transform.position);
                    finalColor += lightColor * influence;
                    totalInfluence += influence;
                }
            }
        }

        return totalInfluence > 0 ? finalColor / totalInfluence : Color.white;
    }

    private void ApplyColor(Color color)
    {
        foreach (RendererMaterials rendererMaterials in rendererMaterialsList)
        {
            ApplyColorToMaterials(color, rendererMaterials, rendererMaterials.isExcluded);
        }
    }

    private static void ApplyColorToMaterials(Color color, RendererMaterials rendererMaterials, bool isExcluded)
    {
        for (int i = 0; i < rendererMaterials.blocks.Length; i++)
        {
            foreach (var kvp in rendererMaterials.originalColors[i])
            {
                if (isExcluded)
                {
                    rendererMaterials.blocks[i].SetColor(kvp.Key, kvp.Value);
                }
                else
                {
                    rendererMaterials.blocks[i].SetColor(kvp.Key, kvp.Value * color);
                }
            }

            rendererMaterials.renderer.SetPropertyBlock(rendererMaterials.blocks[i], i);
        }
    }

    private struct RendererMaterials
    {
        public Renderer renderer;
        public bool isExcluded;
        public Material[] originalMaterials;
        public Dictionary<int, Dictionary<string, Color>> originalColors;
        public MaterialPropertyBlock[] blocks;
    }

    private List<RendererMaterials> rendererMaterialsList = new List<RendererMaterials>();
    private static readonly string[] ColorProperties = { "_Color", "_Color0", "_Color1" };
}