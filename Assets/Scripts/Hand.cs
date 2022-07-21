using UnityEngine;

public class Hand : MonoBehaviour
{
	public Weapon CurrentWeapon;
	public string WeaponName;
	[SerializeField] Sprite arrowSprite;
	Transform weaponHolder;
	SpriteRenderer spriteRenderer;

	bool equipped = false;

	private void Awake()
	{
		weaponHolder = transform.GetChild(0);
		spriteRenderer = weaponHolder.GetComponent<SpriteRenderer>();
	}

	public void SetWeapon(Weapon weapon)
	{
		CurrentWeapon = weapon;
		WeaponName = CurrentWeapon.name;
		Debug.Log("You have set " + WeaponName + " as the current weapon");
	}

	public void RemoveWeapon()
	{
		if (CurrentWeapon == null)
			return;
		UnequipWeapon();
		CurrentWeapon = null;
		WeaponName = null;
		Debug.Log("You have removed the current weapon.");
	}

	public void EquipWeapon()
	{
		if (CurrentWeapon == null || equipped)
			return;
		spriteRenderer.sprite = CurrentWeapon.texture;
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
		CurrentWeapon.Fire((weaponHolder.position - transform.parent.position).normalized, weaponHolder.position, -GetComponentInParent<PlayerController>().cursorAngle);
		Debug.Log("Fired " + WeaponName);
	}
}
