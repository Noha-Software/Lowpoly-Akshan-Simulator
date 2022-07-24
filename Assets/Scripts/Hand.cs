using Kevlaris.Weapons;
using System.Collections;
using UnityEngine;

public class Hand : MonoBehaviour
{
	public Weapon CurrentWeapon;
	public string WeaponName;
	[SerializeField] Sprite arrowSprite;
	Transform weaponHolder;
	SpriteRenderer spriteRenderer;

	bool equipped = false;
	public bool isAutomatic;

	public enum MuzzleType
	{
		None,
		PistolMuzzle
	}

	private void Awake()
	{
		weaponHolder = transform.GetChild(0);
		spriteRenderer = weaponHolder.GetComponent<SpriteRenderer>();
	}

	public void SetWeapon(Weapon weapon)
	{
		CurrentWeapon = weapon;
		WeaponName = CurrentWeapon.name;
		isAutomatic = weapon.automatic;
		CurrentWeapon.canShoot = true;
		Debug.Log("You have set " + WeaponName + " as the current weapon");
	}

	public void RemoveWeapon()
	{
		if (CurrentWeapon == null)
			return;
		UnequipWeapon();
		CurrentWeapon = null;
		WeaponName = null;
		isAutomatic = false;
		Debug.Log("You have removed the current weapon.");
	}

	public void EquipWeapon()
	{
		if (CurrentWeapon == null || equipped)
			return;
		spriteRenderer.sprite = CurrentWeapon.texture;
		CurrentWeapon.roundsUsed = 0;
		CurrentWeapon.canShoot = true;
		equipped = true;
		Debug.Log("You have equipped the current weapon.");
	}

	public void UnequipWeapon()
	{
		if (CurrentWeapon == null || !equipped)
			return;
		spriteRenderer.sprite = arrowSprite;
		equipped = false;
		Debug.Log("You have unequipped the current weapon.");
	}

	public void FireWeapon()
	{
		if (CurrentWeapon == null || !equipped)
			return;

		Vector2 shootPoint;
		switch (CurrentWeapon.muzzleType)
		{
			case MuzzleType.None:
				shootPoint = weaponHolder.position;
				break;
			case MuzzleType.PistolMuzzle:
				shootPoint = weaponHolder.GetChild(0).position;
				break;
			default:
				shootPoint = weaponHolder.position;
				break;
		}

		CurrentWeapon.Fire((weaponHolder.position - transform.parent.position).normalized, shootPoint, -GetComponentInParent<PlayerController>().cursorAngle, this);
		Debug.Log("Fired " + WeaponName);
	}

	public IEnumerator FireCooldown(float fireRate)
	{
		CurrentWeapon.canShoot = false;
		yield return new WaitForSeconds(60f / fireRate);
		CurrentWeapon.canShoot = true;
	}

	public IEnumerator Reload(float reloadRate)
	{
		CurrentWeapon.canShoot = false;
		Debug.Log("Reloading...");
		yield return new WaitForSeconds(reloadRate);
		CurrentWeapon.canShoot = true;
	}
}
