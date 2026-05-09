using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

public class PlayerScript : MonoBehaviour
{
	#region Lifecycle
	private void Start() => InitializeSettings();

	private void FixedUpdate()
	{
		if (!movementLocked) StaminaCheck();
	}

	private void Update()
	{
		ApplyGravity();
		HandleMouseMovement();
		if (!movementLocked) PlayerMove();
		GuiltCheck();
		InitializeMiscellaneous();
	}
	#endregion

	#region Initialization
	private void InitializeSettings()
	{
		gc = FindObjectOfType<GameControllerScript>();
		cc = GetComponent<CharacterController>();
		CamCam = FindObjectOfType<CameraScript>();
		sensitivityActive = PlayerPrefs.GetInt("AnalogMove") == 1;
		height = transform.position.y;
		stamina = maxStamina;
		playerRotation = transform.rotation;
		mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 5);
		principalBugFixer = 1;
		flipaturn = 1f;
	}

	private void InitializeMiscellaneous()
	{
		if (!gc.KF.gamePaused && cc.velocity.magnitude > 0f)
		{
			gc.KF.LockMouse();
		}
		if (jumpRope & ((transform.position - frozenPosition).magnitude >= 45f) && CamCam.jumpHeight < 0.1f)
		{
			DeactivateJumpRope();
			playtime.Disappoint();
		}
		if (sweepingFailsave > 0f)
		{
			sweepingFailsave -= Time.deltaTime;
		}
		else
		{
			sweeping = false;
			hugging = false;
		}
	}
	#endregion

	#region Movement & Rotation
	private void ApplyGravity()
	{
		Vector3 grav = Vector3.zero;
		if (!cc.isGrounded)
		{
			grav.y -= gravity * Time.deltaTime;
		}
		cc.Move(grav * Time.deltaTime);
	}

	private void HandleMouseMovement()
	{
		if (!isForcedToLook)
		{
			MouseMove();
		}
		else
		{
			HandleForcedLook();
		}
	}

	private void MouseMove()
	{
		playerRotation.eulerAngles = new Vector3(playerRotation.eulerAngles.x, playerRotation.eulerAngles.y, fliparoo);
		playerRotation.eulerAngles += Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivity * Time.timeScale * flipaturn;
		transform.rotation = playerRotation;
	}

	private void HandleForcedLook()
	{
		float angleDiff = Mathf.DeltaAngle(transform.eulerAngles.y, Mathf.Atan2(targetToForcelyLookAt.position.x - transform.position.x, targetToForcelyLookAt.position.z - transform.position.z) * Mathf.Rad2Deg);
		if (Mathf.Abs(angleDiff) < 5f)
		{
			LockOnTarget();
		}
		else
		{
			RotateTowardsTarget(angleDiff);
		}
	}

	private void LockOnTarget()
	{
		transform.LookAt(new Vector3(targetToForcelyLookAt.position.x, transform.position.y, targetToForcelyLookAt.position.z));
		playerRotation = transform.rotation;
		movementLocked = false;
		isForcedToLook = false;
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, fliparoo);
	}

	private void RotateTowardsTarget(float angleDiff)
	{
		transform.Rotate(new Vector3(0f, forceLookSpeed * Mathf.Sign(angleDiff * flipaturn) * Time.deltaTime, 0f));
		playerRotation = transform.rotation;
		movementLocked = true;
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, fliparoo);
	}

	private void PlayerMove()
	{
		if (jumpRope && CamCam.jumpHeight <= 0.1f)
		{
			moveDirection = Vector3.zero;
			cc.Move(Vector3.zero);
			return;
		}

		Vector3 movement = Vector3.zero;
		Vector3 lateralMovement = Vector3.zero;

		if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveForward)) movement = transform.forward;
		if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveBackward)) movement = -transform.forward;
		if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveLeft)) lateralMovement = -transform.right;
		if (Singleton<InputManager>.Instance.GetActionKey(InputAction.MoveRight)) lateralMovement = transform.right;

		if (jumpRope) moveDirection *= jumpRopeSpeedMultiplier;

		AdjustSpeedAndSensitivity(movement, lateralMovement);
		HandleSpecialMovement();
		cc.Move(moveDirection);
		secondaryMovementVelocity = new Vector3(cc.velocity.x, 0f, cc.velocity.z);
	}

	private void AdjustSpeedAndSensitivity(Vector3 movement, Vector3 lateralMovement)
	{
		bool isRunning = Singleton<InputManager>.Instance.GetActionKey(InputAction.Run) && stamina > 0f;
		playerSpeed = isRunning ? runSpeed : walkSpeed;
		sensitivity = sensitivityActive ? Mathf.Clamp((movement + lateralMovement).magnitude, 0f, 1f) : 1f;
		moveDirection = (movement + lateralMovement).normalized * playerSpeed * sensitivity * Time.deltaTime;

		if (isRunning && secondaryMovementVelocity.magnitude > 0.1f && !hugging && !sweeping)
		{
			ResetGuilt("running", 0.1f);
		}
	}

	private void HandleSpecialMovement()
	{
		if (jumpRope && CamCam.jumpHeight > 0.1f)
		{
			moveDirection *= jumpRopeSpeedMultiplier;
		}
		else if (sweeping && !bootsActive)
		{
			moveDirection = gottaSweep.velocity * Time.deltaTime + moveDirection * 0.3f;
		}
		else if (hugging && !bootsActive)
		{
			moveDirection = (firstPrize.velocity * 1.2f * Time.deltaTime + (new Vector3(firstPrizeTransform.position.x, height, firstPrizeTransform.position.z) + new Vector3(Mathf.RoundToInt(firstPrizeTransform.forward.x), 0f, Mathf.RoundToInt(firstPrizeTransform.forward.z)) * 3f - transform.position)) * principalBugFixer;
		}
	}
	#endregion

	#region Stamina & Guilt
	private void StaminaCheck()
	{
		if (!isSliding) staminaPending = stamina;

		if (!sweeping && Singleton<InputManager>.Instance.GetActionKey(InputAction.Run) && stamina > 0f && cc.velocity.magnitude > 0.1f)
		{
			stamina -= staminaDrop * Time.fixedDeltaTime;
		}
		else if (stamina < maxStamina && cc.velocity.magnitude < 0.1f)
		{
			stamina += staminaRise * Time.fixedDeltaTime;
		}

		_ = staminaPending / maxStamina * 136f;

		if (staminaPending > 100f && staminaPending <= 200f)
		{
			_ = 136f + (staminaPending - 100f) / 100f * 7f;
		}

		SliderCustomization();
	}

	private void SliderCustomization()
	{
		if (AdditionalGameCustomizer.Instance != null)
		{
			switch (AdditionalGameCustomizer.Instance.StaminaStyle)
			{
				case AdditionalGameCustomizer.StaminaDisplay.Old:
					staminaBar1.value = stamina / maxStamina * 100f;
					break;
				case AdditionalGameCustomizer.StaminaDisplay.PreOld:
					staminaBar2.value = stamina / maxStamina * 100f;
					break;
				case AdditionalGameCustomizer.StaminaDisplay.Normal:
					staminaBar3.value = Mathf.MoveTowards(staminaBar3.value, stamina / maxStamina * 100f, 50f * Time.deltaTime * 6f);
					break;
				case AdditionalGameCustomizer.StaminaDisplay.Vertical:
					staminaBar4.value = Mathf.MoveTowards(staminaBar4.value, stamina / maxStamina * 100f, 50f * Time.deltaTime * 6f);
					break;
				case AdditionalGameCustomizer.StaminaDisplay.Circle:
					staminaBar5.value = Mathf.MoveTowards(staminaBar5.value, stamina / maxStamina * 100f, 50f * Time.deltaTime * 6f);
					break;
			}
		}
	}

	private IEnumerator StaminometerSlide()
	{
		isSliding = true;
		while (Mathf.Abs(stamina - staminaPending) > 0f)
		{
			staminaPending = Mathf.MoveTowards(staminaPending, stamina, slideSpeed);
			yield return null;
		}
		staminaPending = stamina;
		isSliding = false;
		yield break;
	}

	private void GuiltCheck()
	{
		if (guilt > 0f)
		{
			guilt -= Time.deltaTime;
		}
	}

	public void ResetGuilt(string type, float amount)
	{
		if (amount > guilt)
		{
			guilt = amount;
			guiltType = type;
		}
	}

	public void SetStamina(StaminaChangeMode mode, float value)
	{
		if (value < 0f) value = 0f;

		switch (mode)
		{
			case StaminaChangeMode.Add: stamina += value; break;
			case StaminaChangeMode.Remove: stamina -= value; break;
			case StaminaChangeMode.Multiply: stamina *= value; break;
			case StaminaChangeMode.Divide: if (value != 0f) stamina /= value; break;
			case StaminaChangeMode.Set: stamina = value; break;
		}

		if (Mathf.Abs(stamina - staminaPending) >= 10f)
		{
			StartCoroutine(StaminometerSlide());
		}
	}
	#endregion

	#region Triggers & Game Events
	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.name == "Baldi" & !gc.debugMode)
		{
			gameOver = true;
			RenderSettings.skybox = blackSky;
			StartCoroutine(KeepTheHudOff());
		}
		else if (other.transform.name == "Playtime" & !jumpRope & playtime.playCool <= 0f)
		{
			ActivateJumpRope();
		}

		if (other.name == "OfficeTrigger")
		{
			alsoInOffice = true;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.transform.name == "Gotta Sweep")
		{
			sweeping = true;
			sweepingFailsave = 1f;
		}
		else if (other.transform.name == "1st Prize" & firstPrize.velocity.magnitude > 5f)
		{
			hugging = true;
			sweepingFailsave = 1f;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform.name == "Gotta Sweep")
		{
			sweeping = false;
		}
		else if (other.transform.name == "1st Prize")
		{
			hugging = false;
		}
		if (other.name == "OfficeTrigger")
		{
			alsoInOffice = false;
			ResetGuilt("escape", door.lockTime);
		}
	}

	public IEnumerator KeepTheHudOff()
	{
		while (gameOver)
		{
			hud.SetActive(false);
			if (GameObject.Find("JumpRope(Clone)") != null)
			{
				GameObject.Find("JumpRope(Clone)").SetActive(false);
			}
			yield return new WaitForEndOfFrame();
		}
		yield break;
	}

	public void ActivateJumpRope()
	{
		playtime.audioDevice.PlayOneShot(playtime.aud_ReadyGo);
		GameSet = Instantiate(jumpRopeGame);
		GameSet.GetComponent<JumpRopeScript>().cs = CamCam;
		CamCam.jumpHeight = 0f;
		GameSet.GetComponent<JumpRopeScript>().ps = this;
		GameSet.GetComponent<JumpRopeScript>().playtime = playtime;
		jumpRope = true;
		frozenPosition = transform.position;
	}

	public void DeactivateJumpRope()
	{
		if (GameSet != null)
		{
			Destroy(GameSet);
			GameSet = null;
		}
		jumpRope = false;
	}

	public async void ActivateBoots()
	{
		bootsActive = true;
		await Task.Delay(60000);
		bootsActive = false;
	}
	#endregion

	#region Serialized Fields
	[Header("References")]
	public PlaytimeScript playtime;
	[SerializeField] private DoorScript door;
	[SerializeField] private NavMeshAgent gottaSweep, firstPrize;
	[SerializeField] private Transform firstPrizeTransform;
	public Transform targetToForcelyLookAt, ItemDropLocation;
	[SerializeField] private GameObject jumpRopeGame, hud;
	[SerializeField] private Material blackSky;

	[Header("Staminometer References")]
	[SerializeField] private Slider staminaBar1;
	[SerializeField] private Slider staminaBar2, staminaBar3, staminaBar4, staminaBar5;

	[Header("Movement Settings")]
	[SerializeField] private float walkSpeed = 12f;
	[SerializeField] private float runSpeed = 18f, gravity = 2763f;
	public float stamina = 100f, maxStamina = 100f, forceLookSpeed = 246f;
	#endregion

	#region Internal State
	[HideInInspector] public GameControllerScript gc;
	private CharacterController cc;
	private CameraScript CamCam;
	private Quaternion playerRotation;
	private bool sensitivityActive, sweeping;
	private float sensitivity, playerSpeed, mouseSensitivity, jumpRopeSpeedMultiplier = 0.4f;
	private Vector3 moveDirection, secondaryMovementVelocity, frozenPosition;
	private GameObject GameSet;
	public enum StaminaChangeMode { Add, Remove, Multiply, Divide, Set }
	[HideInInspector] public int principalBugFixer;
	[HideInInspector] public string guiltType;
	[HideInInspector] public float height = 3.879999f, sweepingFailsave, staminaPending, slideSpeed = 7f, staminaDrop = 10f, staminaRise = 20f, guilt, fliparoo, flipaturn;
	[HideInInspector] public bool gameOver, jumpRope, hugging, isSliding, bootsActive, alsoInOffice, movementLocked, isForcedToLook;
	#endregion
}