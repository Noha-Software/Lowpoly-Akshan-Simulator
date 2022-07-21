using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
	[Header("Information")]
	public new string name;
	public string description;
	public Sprite texture;

	[Header("Stats")]
	[Range(1,100)] public int damage;
	[Tooltip("Rounds per minute")] [Min(1)] public float fireRate;
	[Min(1)] public int magazineSize;
	[Tooltip("Seconds")] [Min(0)] public float reloadRate;
	//TODO: implement critical hits

	[Header("Projectile")]
	public Transform projectilePrefab;
	public float projectileSpeed;

	public void Fire(Vector3 direction, Vector3 position, float angle)
	{
		Transform projectileTransform = Instantiate(projectilePrefab, position, Quaternion.identity);
		Projectile projectile = projectileTransform.GetComponent<Projectile>();
		projectile.Initialise(direction, projectileSpeed);
		projectileTransform.rotation = Quaternion.Euler(0, 0, angle);
	}
}