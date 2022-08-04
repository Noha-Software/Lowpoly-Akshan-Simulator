using Kevlaris.Weapons;
using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Hand : MonoBehaviour
{
	public Weapon CurrentWeapon;
	public string WeaponName;
	public Sprite arrowSprite;
	Transform weaponHolder;
	PhotonView playerView;

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
		playerView = transform.parent.GetComponent<PhotonView>();
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
		if (CurrentWeapon == null || !playerView.IsMine)
			return;
		UnequipWeapon();
		CurrentWeapon = null;
		WeaponName = null;
		isAutomatic = false;
		Debug.Log("You have removed the current weapon.");
	}

	public void EquipWeapon()
	{
		if (CurrentWeapon == null || equipped || !playerView.IsMine)
			return;
		playerView.RPC("changeWeaponSprite", RpcTarget.AllBuffered);
		CurrentWeapon.roundsUsed = 0;
		CurrentWeapon.canShoot = true;
		equipped = true;
		Debug.Log("You have equipped the current weapon.");
	}

	public void UnequipWeapon()
	{
		if (CurrentWeapon == null || !equipped || !playerView.IsMine)
			return;
		playerView.RPC("setDefaultSprite", RpcTarget.AllBuffered);
		equipped = false;
		Debug.Log("You have unequipped the current weapon.");
	}

	public void FireWeapon()
	{
		if (CurrentWeapon == null || !equipped || !playerView.IsMine)
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
