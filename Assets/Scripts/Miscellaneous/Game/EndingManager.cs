using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndingManager : MonoBehaviour
{
    #region UnityCallbacks
    private void Start() => Game = FindObjectOfType<GameControllerScript>();
    #endregion

    #region PublicMethods
    public void LoadNormalResults()
    {
        GetResults = true;

        Sych.DestroyIfExists("JumpRope(Clone)");
        Sych.DestroyIfExists("CraftersAttacker(Clone)");

        Game.baldiScrpt.stopMoving = true;
        results.SetActive(true);
        AudioListener.pause = true;

        Game.audioDevice.loop = false;
        Game.audioDevice.Stop();
        Game.escapeMusic.Stop();

        Game.playerCharacter.enabled = true;
        Game.playerCollider.enabled = true;

        Game.ObjectsToEnable.ForEach(o => o.SetActive(false));
    }

    public void LoadSecretEnding()
    {
        GetSecret = true;
        StartCoroutine(BlackFlash());

        if (Game.exitEasingCoroutine != null)
        {
            StopCoroutine(Game.exitEasingCoroutine);
            Game.exitEasingCoroutine = null;
        }

        office.SetActive(true);
        NULL.SetActive(true);

        Sych.DestroyIfExists("JumpRope(Clone)");
        Sych.DestroyIfExists("CraftersAttacker(Clone)");
        Game.baldiScrpt.stopMoving = true;

        Game.audioDevice.loop = false;
        Game.audioDevice.Stop();
        Game.escapeMusic.Stop();

        RenderSettings.ambientLight = Color.white;
        ApplySkybox();

        Game.player.transform.position = SecretWarpPoint.transform.position + Vector3.up * Game.player.height;

        Game.ObjectsToEnable.ForEach(o => o.SetActive(false));
        Environment.ForEach(s => s.SetActive(false));

        Game.player.forceLookSpeed = 750f;
        Game.player.targetToForcelyLookAt = SecretWallText;
        Game.player.isForcedToLook = true;
    }
    #endregion

    #region Coroutines
    private IEnumerator BlackFlash()
    {
        black.SetActive(true);
        yield return new WaitForSeconds(0.7f);
        black.SetActive(false);
    }
    #endregion

    #region PrivateHelpers
    private void ApplySkybox()
    {
        if (AdditionalGameCustomizer.Instance == null) return;

        switch (AdditionalGameCustomizer.Instance.currentSkybox)
        {
            case AdditionalGameCustomizer.SkyboxStyle.Day:
                RenderSettings.skybox = AdditionalGameCustomizer.Instance.NormalSky;
                break;
            case AdditionalGameCustomizer.SkyboxStyle.Sunset:
                RenderSettings.skybox = AdditionalGameCustomizer.Instance.TwilightSky;
                break;
            case AdditionalGameCustomizer.SkyboxStyle.Night:
                RenderSettings.skybox = AdditionalGameCustomizer.Instance.NightSky;
                break;
        }
    }
    #endregion

    #region SerializedFields
    [Header("Ending References")]
    [SerializeField] private Transform SecretWallText;
    [SerializeField] private List<GameObject> Environment = new List<GameObject>();
    [SerializeField] private GameObject office, results, black, SecretWarpPoint, NULL;
    #endregion

    #region PrivateFields
    [HideInInspector] public bool GetResults, GetSecret;
    private GameControllerScript Game;
    #endregion
}