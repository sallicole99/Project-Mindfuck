using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	#region Methods
	private void Awake()
	{
		cc = GetComponent<CharacterController>();
		mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
	}

	private void Start()
	{
		Sych.SetCursorLock(true);
		stamina = staminaMax;
		Time.timeScale = 1f;
	}

	private void Update()
	{
		running = Singleton<InputManager>.Instance.GetActionKey(InputAction.Run);
		MouseMove();
		PlayerMove();
		ApplyGravity();
		StaminaUpdate();
	}
	#endregion

	#region Movement
	private void MouseMove()
	{
		playerRotation.eulerAngles = new Vector3(playerRotation.eulerAngles.x, playerRotation.eulerAngles.y);
		playerRotation.eulerAngles += Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivity * Time.timeScale;
		transform.rotation = playerRotation;
	}

	private void PlayerMove()
	{
		float d = walkSpeed;
		if (stamina > 0f & running)
		{
			d = runSpeed;
		}
		Vector3 a = Vector3.zero;
		Vector3 b = Vector3.zero;

		if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveForward))
			b = transform.forward;
		if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveBackward))
			b = -transform.forward;
		if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveLeft))
			a = -transform.right;
		if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveRight))
			a = transform.right;

		sensitivity = Mathf.Clamp((a + b).magnitude, 0f, 1f);
		cc.Move((a + b).normalized * d * sensitivity * Time.deltaTime);
	}

	private void ApplyGravity()
	{
		Vector3 grav = Vector3.zero;
		if (!cc.isGrounded)
		{
			grav.y -= gravity * Time.deltaTime;
		}
		cc.Move(grav * Time.deltaTime);
	}
	#endregion

	#region Stamina
	public void StaminaUpdate()
	{
		if (cc.velocity.magnitude > cc.minMoveDistance)
		{
			if (running)
			{
				stamina = Mathf.Max(stamina - staminaDrop * Time.deltaTime, 0f);
			}
		}
		else if (stamina < staminaMax)
		{
			stamina += staminaRise * Time.deltaTime;
		}
	}
	#endregion

	#region Variables
	[Header("Movement Speed")]
	[SerializeField] private float walkSpeed = 10f;
	[SerializeField] private float runSpeed = 16f, gravity = 2763f;

	[Header("Stamina Management")]
	[SerializeField] private float stamina = 100f;
	[SerializeField] private float staminaDrop = 10f, staminaRise = 10f, staminaMax = 100f;

	private CharacterController cc;
	private float sensitivity, mouseSensitivity;
	private bool running;
	private Quaternion playerRotation;
	#endregion
}