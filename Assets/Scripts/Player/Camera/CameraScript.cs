using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        ps = FindObjectOfType<PlayerScript>();
        offset = transform.position - player.transform.position;
    }

    private void Update()
    {
        if (ps.jumpRope)
        {
            velocity -= gravity * Time.deltaTime;
            jumpHeight += 2.7f * velocity * Time.deltaTime;
            if (jumpHeight <= 0f)
            {
                jumpHeight = 0f;
                if (Singleton<InputManager>.Instance.GetActionKey(InputAction.Jump))
                {
                    velocity = initVelocity;
                }
            }
            jumpHeightV3 = new Vector3(0f, jumpHeight, 0f);
        }

        if (!ps.gc.KF.gamePaused)
        {
            lookBehind = Singleton<InputManager>.Instance.GetActionKey(InputAction.LookBehind) ? 180 : 0;
        }

        if (AdditionalGameCustomizer.Instance?.FreeRoamCamera == true)
        {
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            FreecamLookX -= mouseY;
            FreecamLookX = Mathf.Clamp(FreecamLookX, -90f, 90f);
        }
    }

    private void LateUpdate()
    {
        if (AdditionalGameCustomizer.Instance?.FreeRoamCamera == true)
        {
            if (!ps.gameOver && !ps.jumpRope && !ps.gc.KF.gamePaused && !ps.gc.Math.learningActive)
            {
                transform.position = player.transform.position + offset;
                transform.rotation = player.transform.rotation * Quaternion.Euler(FreecamLookX, lookBehind, 0f);
                return;
            }
            else if (ps.gameOver)
            {
                transform.position = baldi.position + baldi.forward * 2f + Vector3.up * GameOverOffset;
                transform.LookAt(baldi.position + Vector3.up * 5f);
                return;
            }
            else if (ps.jumpRope)
            {
                transform.position = player.transform.position + offset + jumpHeightV3;
                transform.rotation = player.transform.rotation * Quaternion.Euler(FreecamLookX, lookBehind, 0f);
                return;
            }
        }

        if (!ps.gameOver)
        {
            transform.SetPositionAndRotation(player.transform.position + offset + (ps.jumpRope ? jumpHeightV3 : Vector3.zero), player.transform.rotation * Quaternion.Euler(0f, lookBehind, 0f));
        }
        else
        {
            transform.position = baldi.position + baldi.forward * 2f + Vector3.up * GameOverOffset;
            transform.LookAt(baldi.position + Vector3.up * 5f);
        }
    }

    [Header("References & Settings")]
    [SerializeField] private Transform baldi;
    [SerializeField] private float mouseSensitivity = 2f, GameOverOffset = 5f;

    private GameObject player;
    private PlayerScript ps;
    private int lookBehind;
    private Vector3 jumpHeightV3;
    private float FreecamLookX, initVelocity = 5f, velocity, gravity = 10f;
    [HideInInspector] public Vector3 offset;
    [HideInInspector] public float jumpHeight;
}