using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingGun : Tool
{
	Grapple grapple;
	bool isGrappling;
	Transform toolHolder;

	public override string Name => "Grappling Gun";

	public override string Description => "A gun that grapples.";

	public override Sprite Texture => Resources.Load<Sprite>("Tools/grappling_gun.png");

	public override void Use()
	{
		isGrappling = grapple.grappling;
		if (!isGrappling)
		{
			grapple.StartGrapple();
		}
		else
		{
			grapple.EndGrapple();
		}
	}

	private void Awake()
	{
		grapple = GetComponentInParent<Grapple>();
	}

	private void OnEnable()
	{
		toolHolder = transform.GetChild(0);
		toolHolder.GetComponent<SpriteRenderer>().sprite = Texture;
	}
}
