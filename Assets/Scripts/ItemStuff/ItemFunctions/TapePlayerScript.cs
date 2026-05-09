using UnityEngine;

public class TapePlayerScript : MonoBehaviour
{
    #region Initialization
    private void Start()
    {
        audioDevice = GetComponent<AudioSource>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }
    #endregion

    #region Per-Frame Logic
    private void Update()
    {
        if (!baldi.antiHearing)
        {
            sprite.sprite = openSprite;
        }
    }
    #endregion

    #region Public Methods
    public void Play()
    {
        sprite.sprite = closedSprite;
        audioDevice.Play();

        if (baldi.isActiveAndEnabled)
        {
            baldi.ActivateAntiHearing(30f);
        }
    }
    #endregion

    #region Serialized Configuration
    [Header("References")]
    [SerializeField] private BaldiScript baldi;
    [SerializeField] private Sprite openSprite, closedSprite;
    #endregion

    #region Internal State
    private SpriteRenderer sprite;
    private AudioSource audioDevice;
    #endregion
}