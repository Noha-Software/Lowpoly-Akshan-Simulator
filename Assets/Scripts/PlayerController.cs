using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : NetworkBehaviour
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
	float horizontalValue;

	[Header("Arrow")]
	[SerializeField] public Transform arrow;
	[SerializeField] [Min(0)] float arrowRotationSpeed = 500;
	[SerializeField] [Range(0,360)] float arrowAngle;
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
		if (!base.IsOwner)
		{
			return;
		}

		if (!isPaused)
		{
			GetInputs();

			GroundCheck();

			Move(horizontalValue);

			RotateArrow();
		}
		Accelerate();
	}
	
	[Client(RequireOwnership = true)]
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
	}

	[Client(RequireOwnership = true)]
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

	[Client(RequireOwnership = true)]
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

	[Client(RequireOwnership = true)]
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
	[Client(RequireOwnership = true)]
	void RotateArrow()
	{
		Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		float distance = Vector2.Distance(transform.position, mouse);
		if (distance > 20f) distance = 20f;

		arrowAngle = transform.eulerAngles.z % 360f;
		if (arrowAngle < 0f) arrowAngle += 360f;
		else if (arrowAngle > 360f) arrowAngle -= 360f;

		Vector2 deltaVector = (mouse - transform.position).normalized;

		Quaternion forwardRotation = Quaternion.AngleAxis(arrowAngle, Vector3.forward);
		Vector2 forwardDirection = forwardRotation * new Vector2(distance, 0f);

		arrowAngle = Vector2.SignedAngle(deltaVector, forwardDirection);
		arrow.rotation = Quaternion.Lerp(arrow.rotation, Quaternion.Euler(0, 0, -arrowAngle + arrowRotationSpeed * Time.fixedDeltaTime), Time.fixedDeltaTime * damping);
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