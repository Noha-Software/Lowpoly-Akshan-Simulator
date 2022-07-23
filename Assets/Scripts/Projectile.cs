using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	Vector3 direction;
	float speed;
	int damage;
	bool init;
	Collider2D trigger;
	public void Initialise(Vector3 direction, float speed, int damage)
	{
		this.direction = direction;
		this.speed = speed;
		this.damage = damage;
		StartCoroutine(BulletTimeout());
		init = true;
		trigger = GetComponent<Collider2D>();
		trigger.isTrigger = true;
	}

	private void FixedUpdate()
	{
		if (!init) return;
		transform.position += direction * speed * Time.deltaTime;
	}

	IEnumerator BulletTimeout()
	{
		yield return new WaitForSeconds(2f);
		Destroy(gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		PlayerController player = collision.GetComponent<PlayerController>();
		if (player != null)
		{
			Debug.Log("Player hit!");
			player.Damage(damage);
		}

		Destroy(gameObject);
	}
}
