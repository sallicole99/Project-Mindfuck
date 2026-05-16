using UnityEngine;
using System.Collections.Generic;

namespace JohnsterSpaceTools
{
    public class TileLighting : MonoBehaviour
    {
        public void Start()
        {
            if (setOnStart)
            {
                GetTileRenderers();
                SetLighting();
            }
            else
            {
                if (destroyLighting)
                {
                    Destroy(this);
                }
            }
        }

        public void GetTileRenderers()
        {
            if (tileRenderers.Count <= 0)
            {
                foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
                {
                    tileRenderers.Add(r);
                }
            }
        }

        public void SetLighting()
        {
            lightStrength = Random.Range(minLightStrength, maxLightStrength);
            UpdateLighting(lightStrength, lightStrength, lightStrength);
        }

        public void UpdateLighting(float redStrength, float greenStrength, float blueStrength)
        {
            if (tileLightColor == TileLightColor.White)
            {
                redStrength = lightStrength; greenStrength = lightStrength; blueStrength = lightStrength;
            }
            else if (tileLightColor == TileLightColor.Red)
            {
                redStrength = lightStrength; greenStrength = 0f; blueStrength = 0f;
            }
            else if (tileLightColor == TileLightColor.Green)
            {
                redStrength = 0f; greenStrength = lightStrength; blueStrength = 0f;
            }
            else if (tileLightColor == TileLightColor.Blue)
            {
                redStrength = 0f; greenStrength = 0f; blueStrength = lightStrength;
            }
            else if (tileLightColor == TileLightColor.Random)
            {
                redStrength = Random.Range(0f, 100f); greenStrength = Random.Range(0f, 100f); blueStrength = Random.Range(0f, 100f);
            }

            if (tileLightColor != TileLightColor.Custom)
            {
                lightColor = new Color(redStrength / 100f, greenStrength / 100f, blueStrength / 100f, 1f);
            }
            else
            {
                lightColor = new Color(lightColor.r, lightColor.g, lightColor.b, 1f);
            }

            foreach (Renderer r in tileRenderers)
            {
                for (int i = 0; i < r.materials.Length; i++)
                {
                    r.materials[i].color = lightColor;
                }
            }
        }

        public enum TileLightColor
        {
            White, Red, Green, Blue, Random, Custom
        }

        public Color lightColor = Color.white;
        private float lightStrength;
        public List<Renderer> tileRenderers;

        public float minLightStrength = 100f;
        public float maxLightStrength = 100f;

        public TileLightColor tileLightColor;
        public bool destroyLighting;
        public bool setOnStart;
    }
}
