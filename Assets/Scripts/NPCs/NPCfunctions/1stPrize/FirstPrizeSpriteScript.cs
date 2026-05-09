using UnityEngine;

public class FirstPrizeSpriteScript : MonoBehaviour
{
    private void Start() => spriteRenderer = GetComponent<SpriteRenderer>();

    private void Update() => UpdateSprite();

    private void UpdateSprite()
    {
        Vector3 delta = cameraTransform.position - bodyTransform.position;
        float angle = Mathf.Repeat(Mathf.Atan2(delta.z, delta.x) * Mathf.Rad2Deg + bodyTransform.eulerAngles.y, 360f);
        int index = Mathf.RoundToInt(angle / 22.5f) % 16;
        spriteRenderer.sprite = sprites[index];
        debug = angle;
    }

    [Header("Components & References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform bodyTransform;

    [Header("Sprite & Angle Settings")]
    [SerializeField] private Sprite[] sprites = new Sprite[16];

    [Header("Debugging")]
    public float debug;
    
    private SpriteRenderer spriteRenderer;
}