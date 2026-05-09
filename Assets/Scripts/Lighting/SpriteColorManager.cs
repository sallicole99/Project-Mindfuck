using UnityEngine;

public class SpriteColorManager : MonoBehaviour
{
    private void Start()
    {
        layerMask = LayerMask.GetMask("Floor");
        InitializeSpriteRenderer();
        if (!useOnRuntime)
        {
            SetColor();
        }
    }

    private void InitializeSpriteRenderer()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteMaterial = spriteRenderer.material;
            originalSpriteColor = spriteMaterial.GetColor("_Color");
        }
    }

    private void OnDestroy() => CleanupSpriteRenderer();

    private void CleanupSpriteRenderer()
    {
        if (spriteMaterial != null)
        {
            spriteMaterial.SetColor("_Color", originalSpriteColor);
        }
    }

    private void LateUpdate()
    {
        if (spriteRenderer != null && Time.timeScale != 0f && spriteRenderer.isVisible && useOnRuntime)
        {
            SetColor();
        }
    }

    private void SetColor()
    {
        Color mixedColor = originalSpriteColor * GetTileColor();
        spriteMaterial.SetColor("_Color", mixedColor);
    }

    private Color GetTileColor()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, float.PositiveInfinity, layerMask, QueryTriggerInteraction.Ignore))
        {
            TileColorManager curTile = hit.transform.parent.GetComponent<TileColorManager>();
            if (curTile != null)
            {
                return curTile.GetTileColor();
            }
        }
        return Color.white;
    }

    [SerializeField] private bool useOnRuntime = true;

    private SpriteRenderer spriteRenderer;
    private Material spriteMaterial;
    private Color originalSpriteColor;
    private LayerMask layerMask;
}
