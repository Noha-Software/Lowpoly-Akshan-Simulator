using System.Collections;
using UnityEngine;
using Kevlaris.UI;
using Kevlaris.Weapons;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
	Rigidbody2D rb;
	Animator animator;
	Grapple grapple;
	SpriteRenderer spriteRenderer;

	[Header("Detection")]
	[SerializeField] Collider2D standingCollider;
	[SerializeField] Transform groundCollider;
	[SerializeField] LayerMask groundLayer;
	const float groundCheckRadius = 0.2f;
	[SerializeField] bool controlled = true;

	[Header("Movement")]
	[SerializeField] float speed = 30f;
	[SerializeField] float jumpPower = 7f;
	[SerializeField] float fallMultiplier = 2.5f;
	[SerializeField] float lowJumpMultiplier = 2f;
	[SerializeField] float coyoteJumpWindow = 0.2f;
	[SerializeField] bool animate;
	float horizontalValue;

	[Header("Cursor")]
	public Hand hand;
	[Range(-180,180)] public float cursorAngle;
	[SerializeField] [Min(0)] float damping = 100;

	[Header("Other")]
	[SerializeField] Weapon weapon;
	[SerializeField] int maxHealth = 100;
	[SerializeField] ProgressBar healthBar;
	bool isGrounded = false;
	bool coyoteJump;
	bool isPaused;
	Color originalColor;

	public int Health { get; private set; }

	private void OnApplicationFocus(bool focus)
	{
		isPaused = !focus;
	}

	void Awake()
	{
		//Get 2D RigidBody and Animator
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		Health = maxHealth;
		originalColor = spriteRenderer.color;

		healthBar.Initialise(Color.white, Color.red, maxHealth);
		healthBar.SetValue(maxHealth);

		if (!controlled) return;

		grapple = GetComponent<Grapple>();
		hand.SetWeapon(weapon);
	}

	void FixedUpdate()
	{
		if (!controlled) return;
		if (!isPaused)
		{
			GetInputs();
			Move(horizontalValue);
			RotateCursor();
		}
		GroundCheck();
		Accelerate();
	}

	void GetInputs()
	{
		if (hand.isAutomatic)
		{
			if (Input.GetButton("Fire1"))
				hand.FireWeapon();
		}
		else
		{
			if (Input.GetButtonDown("Fire1"))
				hand.FireWeapon();
		}

		if (Input.GetButtonDown("Equip"))
			hand.EquipWeapon();

		if (Input.GetButtonDown("Unequip"))
			hand.UnequipWeapon();

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
	void RotateCursor()
	{
		Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		float distance = Vector2.Distance(transform.position, mouse);

		float angle = transform.rotation.eulerAngles.z % 360f;
		if (angle < 0f) angle += 360f;
		else if (angle > 360f) angle -= 360f;

		Vector2 deltaVector = (mouse - transform.position).normalized;

		Quaternion forwardRotation = Quaternion.AngleAxis(angle, Vector3.forward);
		Vector2 forwardDirection = forwardRotation * new Vector2(distance, 0f);

		cursorAngle = Vector2.SignedAngle(deltaVector, forwardDirection);
		hand.transform.rotation = Quaternion.Lerp(hand.transform.rotation, Quaternion.Euler(0, 0, -cursorAngle), Time.fixedDeltaTime * damping);

		if (cursorAngle > 90 || cursorAngle < -90)
		{
			hand.transform.GetChild(0).GetComponent<SpriteRenderer>().flipY = true;
		}
		else
		{
			hand.transform.GetChild(0).GetComponent<SpriteRenderer>().flipY = false;
		}
	}

	public void Damage(int dmg)
	{
		Health -= dmg;
		healthBar.SetValue(Health);
		StopCoroutine("Flash");
		spriteRenderer.color = originalColor;
		StartCoroutine(Flash(Color.red, .5f));
		if (Health <= 0)
			Die();
	}

	IEnumerator Flash(Color color, float t)
	{
		float time = 0;
		while (time < t/2)
		{
			spriteRenderer.color = Color.Lerp(spriteRenderer.color, color, time / (t / 2));
			time += Time.deltaTime;
			yield return null;
		}
		spriteRenderer.color = color;
		time = 0;
		while (time < t/2)
		{
			spriteRenderer.color = Color.Lerp(spriteRenderer.color, originalColor, time / (t / 2));
			time += Time.deltaTime;
			yield return null;
		}
		spriteRenderer.color = originalColor;
	}

	public void Heal(int amount)
	{
		if (Health + amount >= maxHealth)
			Health = maxHealth;
		else
			Health += amount;
		healthBar.SetValue(Health);
		StartCoroutine(Flash(Color.green, 1f));
	}

	void Die()
	{
		Debug.Log("YOU DIED");
		ResetPlayer();
	}

	void ResetPlayer()
	{
		if (grapple != null)
		{
			if (grapple.grappling) grapple.ToggleGrapple();
		}
		transform.position = Vector3.zero;
		Health = maxHealth;
		healthBar.SetValue(maxHealth);
		hand.UnequipWeapon();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(groundCollider.position, groundCheckRadius);
	}
}