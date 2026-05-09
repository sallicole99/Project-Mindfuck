using TMPro;
using UnityEngine;

public class JumpRopeScript : MonoBehaviour
{
	#region Methods
	private void OnEnable()
	{
		KeyCode jumpKey = Singleton<InputManager>.Instance.KeyboardMapping[InputAction.Jump];
		Instructions.text = $"Time to jump rope!\nUse {jumpKey} to jump!";

		jumpDelay = 1f;
		ropeHit = true;
		jumpStarted = false;
		jumps = 0;

		if (AdditionalGameCustomizer.Instance.RandomizeJumps)
			maxJumps = Random.Range(1, 10);

		jumpCount.text = $"{jumps}/{maxJumps}";
	}

	private void Update()
	{
		if (jumpDelay > 0f)
		{
			jumpDelay -= Time.deltaTime;
			return;
		}

		if (!jumpStarted)
		{
			if (jumps >= maxJumps)
			{
				EndJumpRope();
				return;
			}

			StartJumpRope();
		}

		if (ropePosition > 0f)
		{
			ropePosition -= Time.deltaTime;
		}
		else if (!ropeHit)
		{
			RopeHit();
		}
	}
	#endregion

	#region Core Logic
	private void StartJumpRope()
	{
		jumpStarted = true;
		ropePosition = 1f;
		rope.SetTrigger("ActivateJumpRope");
		ropeHit = false;
	}

	private void RopeHit()
	{
		ropeHit = true;

		if (cs.jumpHeight <= 0.2f)
			Fail();
		else
			Success();

		jumpStarted = false;
	}

	private void Success()
	{
		playtime.audioDevice.Stop();
		playtime.audioDevice.PlayOneShot(playtime.aud_Numbers[jumps]);

		jumps++;
		jumpCount.text = $"{jumps}/{maxJumps}";
		jumpDelay = 0.5f;
	}

	private void Fail()
	{
		jumps = 0;
		jumpCount.text = $"{jumps}/{maxJumps}";
		jumpDelay = 2f;

		playtime.audioDevice.Stop();
		playtime.audioDevice.PlayOneShot(playtime.aud_Oops);
	}

	private void EndJumpRope()
	{
		playtime.canTargetPlayer = true;
		playtime.jumpRopeStarted = false;
		playtime.playCool = 15f;

		playtime.audioDevice.PlayOneShot(playtime.aud_Congrats);
		ps.DeactivateJumpRope();
		playtime.ResumeWandering();
	}
	#endregion

	#region SerializedFields
	[Header("UI Elements")]
	[SerializeField] private TMP_Text jumpCount;
	[SerializeField] private TMP_Text Instructions;

	[Header("Rope Animation")]
	[SerializeField] private Animator rope;

	[Header("Player and Playtime Scripts")]
	public CameraScript cs;
	public PlayerScript ps;
	public PlaytimeScript playtime;

	[Header("Jump Settings")]
	[SerializeField] private int maxJumps;
	#endregion

	#region Internal State
	private int jumps;
	private bool ropeHit, jumpStarted;
	private float jumpDelay, ropePosition;
    #endregion
}