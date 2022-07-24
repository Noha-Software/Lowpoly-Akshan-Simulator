using UnityEngine;

namespace Kevlaris.Weapons
{
	[System.Serializable]
	[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
	public class Weapon : ScriptableObject
	{
		[Header("Information")]
		public new string name;
		public string description;
		public Sprite texture;

		[Header("Stats")]
		[Range(1, 100)] public int damage;
		[Tooltip("Rounds per minute")] [Min(1)] public int fireRate;
		[Min(1)] public int magazineSize;
		[Tooltip("Seconds")] [Min(0)] public float reloadRate;
		[Tooltip("Can the fire button be held down to fire automatically?")] public bool automatic = true;
		[Tooltip("Chance of critical strikes (0 = disabled)")] [Range(0, 100)] public int criticalChance;
		[Tooltip("Percentage of the original damage to deal upon critical strikes (x = 100% + x damage)")] [Min(0)] public int criticalDamage;

		[Header("Projectile")]
		public Transform projectilePrefab;
		public float projectileSpeed;
		[Tooltip("Position of muzzle")] public Hand.MuzzleType muzzleType;

		[Header("Other")]
		[HideInInspector] public bool canShoot = true;
		[HideInInspector] public int roundsUsed = 0;

		public void Fire(Vector3 direction, Vector3 position, float angle, Hand hand)
		{
			if (!canShoot) return;
			if (roundsUsed >= magazineSize)
			{
				hand.StartCoroutine(hand.Reload(reloadRate));
				roundsUsed = 0;
				return;
			}
			Transform projectileTransform = Instantiate(projectilePrefab, position, Quaternion.identity);
			Projectile projectile = projectileTransform.GetComponent<Projectile>();
			projectile.Initialise(direction, projectileSpeed, this);
			projectileTransform.rotation = Quaternion.Euler(0, 0, angle);
			roundsUsed++;
			hand.StartCoroutine(hand.FireCooldown(fireRate));
		}

		public void OnPlayerHit(PlayerController player)
		{
			int dmg = damage;
			if (criticalChance > 0)
			{
				float rnd = Random.Range(0f, 100f);
				if (rnd < criticalChance)
				{
					dmg *= 1 + (criticalDamage / 100);
					Debug.Log("Critical hit for " + dmg + " damage");
				}
			}
			player.Damage(dmg);
		}
	}
}