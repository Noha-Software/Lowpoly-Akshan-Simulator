using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
	Rigidbody2D rb;
	Animator animator;
	Grapple grapple;

	[Header("Detection")]
	[SerializeField] Collider2D standingCollider;
	[SerializeField] Transform groundCollider;
	[SerializeField] LayerMask groundLayer;
	const float groundCheckRadius = 0.2f;

	[Header("Movement")]
	[SerializeField] float speed = 30f;
	[SerializeField] float jumpPower = 7f;
	[SerializeField] float fallMultiplier = 2.5f;
	[SerializeField] float lowJumpMultiplier = 2f;
	[SerializeField] float coyoteJumpWindow = 0.2f;
	[SerializeField] bool animate;
	float horizontalValue;

	[Header("Tool")]
	//[SerializeField] public Transform toolPivot;
	[SerializeField] public Hand hand;
	[SerializeField] [Min(0)] float toolRotationSpeed = 500;
	[SerializeField] [Range(0,360)] float toolAngle;
	[SerializeField] [Min(0)] float damping = 100;

	[Header("Other")]
	public bool isDead = false;
	public bool lostGame = false;
	bool isGrounded = false;
	bool coyoteJump;
	bool isPaused;

	private void OnApplicationFocus(bool focus)
	{
		isPaused = !focus;
	}

	void Awake()
	{
		//Get 2D RigidBody and Animator
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		grapple = GetComponent<Grapple>();
	}

	void FixedUpdate()
	{
		if (!isPaused)
		{
			GetInputs();
			Move(horizontalValue);
			RotateTool();
		}
		GroundCheck();
		Accelerate();
	}

	void GetInputs()
	{
		if (!CanMove())
		{
			horizontalValue = 0f;
			return;
		}

		//Set animator's Y velocity
		animator.SetFloat("YVelocity", rb.velocity.y);

		horizontalValue = Input.GetAxisRaw("Horizontal");

		if (Input.GetButtonDown("Jump"))
			Jump();

		if (Input.GetMouseButton(0))
		{
			if (hand.currentTool != null)
				hand.currentTool.Use();
		}

		if (Input.GetKeyDown(KeyCode.F))
			hand.SetTool(typeof(GrapplingGun));
		if (Input.GetKeyDown(KeyCode.G))
			hand.RemoveTool();
	}

	void Accelerate()
	{
		if (rb.velocity.y < 0)
		{
			rb.velocity += Vector2.up * Physics2D.gravity.y * Time.fixedDeltaTime * (fallMultiplier - 1);
		}
		else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
		{
			rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
		}
	}

	/// <summary>
	/// Checks if the player is currently able to move
	/// </summary>
	public bool CanMove()
	{
		bool can = true;
		if (isDead)
			can = false;
		if (lostGame)
			can = false;
		if (grapple.grappling)
			can = false;

		return can;
	}

	/// <summary>
	/// Checks if the player is on the ground
	/// </summary>
	void GroundCheck()
	{
		bool wasGrounded = isGrounded;
		isGrounded = false;

		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCollider.position, groundCheckRadius, groundLayer);
		
		if(colliders.Length > 0)
		{
			isGrounded = true;
		}
		else
		{
			//Coyote Jump
			if (wasGrounded)
				StartCoroutine(CoyoteJumpDelay());
		}

		animator.SetBool("Jump", !isGrounded);
	}

	IEnumerator CoyoteJumpDelay()
	{
		coyoteJump = true;
		yield return new WaitForSeconds(coyoteJumpWindow);
		coyoteJump = false;
	}

	void Jump()
	{
		//if on ground, jump
		if (isGrounded)
		{
			rb.velocity = Vector2.up * jumpPower;
			animator.SetBool("Jump", true);
		}
		else
		{
			//if can coyotejump, jump
			if (coyoteJump)
			{
				rb.velocity = Vector2.up * jumpPower;
				animator.SetBool("Jump", true);
			}
		}
	}

	void Move(float dir)
	{
		float xVal = dir * speed * 10 * Time.fixedDeltaTime;

		Vector2 targetVelocity = new Vector2(xVal, rb.velocity.y);

		rb.velocity = targetVelocity;

		// 0 = idle, 1 = walking, 2 = running
		// set the animator float according to the absolute of velocity
		animator.SetFloat("XVelocity", Mathf.Abs(rb.velocity.x));
	}

	/// <summary>
	/// Rotate the attached arrow to face the mouse
	/// </summary>
	void RotateTool()
	{
		Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		float distance = Vector2.Distance(transform.position, mouse);
		if (distance > 20f) distance = 20f;

		toolAngle = transform.eulerAngles.z % 360f;
		if (toolAngle < 0f) toolAngle += 360f;
		else if (toolAngle > 360f) toolAngle -= 360f;

		Vector2 deltaVector = (mouse - transform.position).normalized;

		Quaternion forwardRotation = Quaternion.AngleAxis(toolAngle, Vector3.forward);
		Vector2 forwardDirection = forwardRotation * new Vector2(distance, 0f);

		toolAngle = Vector2.SignedAngle(deltaVector, forwardDirection);
		hand.transform.rotation = Quaternion.Lerp(hand.transform.rotation, Quaternion.Euler(0, 0, -toolAngle + toolRotationSpeed * Time.fixedDeltaTime), Time.fixedDeltaTime * damping);
	}

	public void ResetPlayer()
	{
		horizontalValue = 0f;
	}


	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(groundCollider.position, groundCheckRadius);
	}
}