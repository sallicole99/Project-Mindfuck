using UnityEngine;
 
public class CameraScript : MonoBehaviour
{
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        ps = FindObjectOfType<PlayerScript>();
        offset = transform.position - player.transform.position;
        cam = Camera.main;
        if (cam != null)
        {
            cam.fieldOfView = baseFOV;
            currentFOV = baseFOV;
        }
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
                transform.rotation = player.transform.rotation * Quaternion.Euler(FreecamLookX + bobPitch, lookBehind, bobRoll + damageTilt);
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
                transform.rotation = player.transform.rotation * Quaternion.Euler(FreecamLookX + bobPitch, lookBehind, bobRoll + damageTilt);
                return;
            }
        }
 
        if (!ps.gameOver)
        {
            transform.SetPositionAndRotation(
                player.transform.position + offset + (ps.jumpRope ? jumpHeightV3 : Vector3.zero),
                player.transform.rotation * Quaternion.Euler(bobPitch, lookBehind, bobRoll + damageTilt)
            );
        }
        else
        {
            transform.position = baldi.position + baldi.forward * 2f + Vector3.up * GameOverOffset;
            transform.LookAt(baldi.position + Vector3.up * 5f);
        }

        UpdateBob();
        UpdateFOV();
    }
 
    private void UpdateFOV()
    {
        if (cam == null || ps == null) return;
 
        bool shiftHeld = Singleton<InputManager>.Instance.GetActionKey(InputAction.Run);
        bool tabHeld   = Input.GetKey(KeyCode.Tab);
        bool isMoving  = ps.GetComponent<CharacterController>().velocity.magnitude > 0.1f;
 
        float targetFOV = baseFOV;
        if (shiftHeld && tabHeld && isMoving)
            targetFOV = sprintFOV;
        else if (shiftHeld && !tabHeld && isMoving)
            targetFOV = runFOV;
 
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovLerpSpeed);
        cam.fieldOfView = currentFOV;
        damageTilt = Mathf.Lerp(damageTilt, 0f, Time.deltaTime * damageTiltSpeed);
    }
 
    public void TriggerDamageTilt()
    {
        damageTilt = damageTiltAmount;
    }
 
    private void UpdateBob()
    {
        if (ps == null) return;
 
        bool shiftHeld = Singleton<InputManager>.Instance.GetActionKey(InputAction.Run);
        bool tabHeld   = Input.GetKey(KeyCode.Tab);
        bool isMoving  = ps.GetComponent<CharacterController>().velocity.magnitude > 0.1f;
 
        if (!isMoving)
        {
            bobTimer = 0f;
            bobPitch = Mathf.Lerp(bobPitch, 0f, Time.deltaTime * bobLerpSpeed);
            bobRoll  = Mathf.Lerp(bobRoll,  0f, Time.deltaTime * bobLerpSpeed);
            return;
        }
 
        float bobSpeed, pitchAmount, rollAmount;
 
        if (shiftHeld && tabHeld)
        {
            bobSpeed    = sprintBobSpeed;
            pitchAmount = sprintBobAmount * 0.5f;
            rollAmount  = sprintBobAmount;
        }
        else if (shiftHeld)
        {
            bobSpeed    = runBobSpeed;
            pitchAmount = runBobAmount;
            rollAmount  = runBobAmount * 1.5f;
        }
        else
        {
            bobSpeed    = walkBobSpeed;
            pitchAmount = walkBobAmount;
            rollAmount  = walkBobAmount * 1.5f;
        }
 
        bobTimer += Time.deltaTime * bobSpeed;
 
        float targetPitch = Mathf.Sin(bobTimer) * pitchAmount;
        float targetRoll  = Mathf.Sin(bobTimer * 0.5f) * rollAmount;
 
        bobPitch = Mathf.Lerp(bobPitch, targetPitch, Time.deltaTime * bobLerpSpeed);
        bobRoll  = Mathf.Lerp(bobRoll,  targetRoll,  Time.deltaTime * bobLerpSpeed);
    }
 
    [Header("References & Settings")]
    [SerializeField] private Transform baldi;
    [SerializeField] private float mouseSensitivity = 2f, GameOverOffset = 5f;
 
    [Header("FOV Settings")]
    public float baseFOV      = 70f;
    public float runFOV       = 80f;
    public float sprintFOV    = 100f;
    public float fovLerpSpeed = 8f;
 
    [Header("View Bob Settings")]
    public float walkBobSpeed    = 8f;
    public float walkBobAmount   = 0.8f;
    public float runBobSpeed     = 14f;
    public float runBobAmount    = 1.4f;
    public float sprintBobSpeed  = 10f;
    public float sprintBobAmount = 0.6f;
    public float bobLerpSpeed    = 10f;
 
    [Header("Damage Tilt")]
    public float damageTiltAmount = 15f;  // degrees to tilt
    public float damageTiltSpeed  = 10f;  // how fast it recovers
    private float damageTilt      = 0f;   // current tilt value

    private Camera cam;
    private float currentFOV;
    private float bobTimer = 0f;
    private float bobPitch = 0f;
    private float bobRoll  = 0f;
    private GameObject player;
    private PlayerScript ps;
    private int lookBehind;
    private Vector3 jumpHeightV3;
    private float FreecamLookX, initVelocity = 5f, velocity, gravity = 10f;
    [HideInInspector] public Vector3 offset;
    [HideInInspector] public float jumpHeight;
}