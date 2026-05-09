using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyFunctions : MonoBehaviour
{
    #region UnityCallbacks
    private void Start() => LockMouse();

    private void Update()
    {
        if (!gamePaused)
        {
            ItemCollecting();
        }

        PauseAndExit();
    }
    #endregion

    #region PauseAndExitManagement
    private void PauseAndExit()
    {
        if (gc.Math.learningActive || gc.player.gameOver) return;

        if (Singleton<InputManager>.Instance.GetActionKey(InputAction.PauseOrCancel) && !gc.progress.GetResults)
        {
            ToggleGamePause(!gamePaused);
        }

        if (gamePaused)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                ExitGame();
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                ToggleGamePause(false);
            }
        }
    }

    public void ToggleGamePause(bool isPaused)
    {
        Time.timeScale = isPaused ? 0f : 1f;
        AudioListener.pause = isPaused;
        gamePaused = isPaused;
        pauseMenu.SetActive(isPaused);

        if (isPaused)
        {
            UnlockMouse();
        }
        else
        {
            LockMouse();
        }
    }

    public void ExitGame()
    {
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region ItemInteraction
    private void ItemCollecting()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetMouseButtonDown(0) || Singleton<InputManager>.Instance.GetActionKey(InputAction.Interact))
        {
            if (Sych.ScreenCenterRaycast(out RaycastHit hit) && hit.transform.IsWithinDistance(10f))
            {
                if (hit.collider.TryGetComponent(out Interactable interactable))
                {
                    interactable.Interact();
                }
            }
        }
    }
    #endregion

    #region CursorControl
    public void LockMouse()
    {
        if (!gc.Math.learningActive)
        {
            Sych.SetCursorLock(true);
            mouseLocked = true;
            reticle.SetActive(true);
        }
    }

    public void UnlockMouse()
    {
        Sych.SetCursorLock(false);
        mouseLocked = false;
        reticle.SetActive(false);
    }
    #endregion

    #region SerializedFields
    [SerializeField] private GameObject pauseMenu, reticle;
    [SerializeField] private GameControllerScript gc;
    #endregion

    #region PublicState
    [HideInInspector] public bool mouseLocked, gamePaused;
    #endregion
}