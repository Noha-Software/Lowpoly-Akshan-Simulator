using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	Vector3 direction;
	float speed;
	bool init;
    public void Initialise(Vector3 direction, float speed)
	{
		this.direction = direction;
		this.speed = speed;
		StartCoroutine(BulletTimeout());
		init = true;
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
}
