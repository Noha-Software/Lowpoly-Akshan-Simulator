using System.Collections;
using UnityEngine;

public class Grapple : MonoBehaviour
{
	public bool grappling = false;
	public bool canGrapple = true;
	PlayerController player;

	[Header("Targeting")]
	[SerializeField] bool targetAll = false;
	[SerializeField] LayerMask targetLayer;
	[SerializeField] [Min(1)] [Tooltip("Maximum distance to grapple")] int targetDistance = 15;

	[Header("Rope")]
	[SerializeField] LineRenderer lineRenderer;
	[SerializeField] [Range(0, 1)] [Tooltip("Length of rope compared to distance to grapple point")] float length = .7f;
	[SerializeField] bool launch = false;
	[SerializeField] [Min(0)] float ropeLerpTime = .3f;
	Vector3 grapplePoint;
	//Camera cam;

	private void Awake()
	{
		//cam = Camera.main;
		lineRenderer.positionCount = 2;
		player = GetComponent<PlayerController>();
	}

	private void FixedUpdate()
	{
		if (Input.GetButtonDown("Fire2"))
			ToggleGrapple();
	}

	private void LateUpdate()
	{
		if (grappling)
			lineRenderer.SetPosition(0, transform.position);
	}

	public void ToggleGrapple()
	{
		if (!canGrapple)
			return;
		if (!grappling)
		{
			StartGrapple();
		}
		else
		{
			EndGrapple();
		}
	}

	void StartGrapple()
	{
		canGrapple = false;

		//Vector3 screenPoint = Input.mousePosition;
		//screenPoint.z = transform.position.z;
		//Vector3 worldPoint = cam.ScreenToWorldPoint(screenPoint);
		Vector3 origin = transform.position;
		Vector3 direction = player.hand.transform.GetChild(0).position - origin;

		RaycastHit2D hit;
		if (targetAll)
		{
			hit = Physics2D.Raycast(origin, direction, targetDistance);
		}
		else
		{
			hit = Physics2D.Raycast(origin, direction, targetDistance, targetLayer);
		}

		if (!hit)
		{
			canGrapple = true;
			return;
		}

		grapplePoint = hit.point;

		SpringJoint2D springJoint = gameObject.AddComponent<SpringJoint2D>();
		springJoint.autoConfigureDistance = false;
		if (launch)
		{
			springJoint.distance = 1f;
			springJoint.frequency = 0f;
			springJoint.dampingRatio = .5f;
		}
		else
		{
			springJoint.distance = Vector2.Distance(transform.position, grapplePoint) * length;
			springJoint.frequency = .8f;
			springJoint.dampingRatio = 0.2f;
		}
		springJoint.enableCollision = true;
		springJoint.connectedBody = hit.rigidbody;
		springJoint.connectedAnchor = hit.transform.InverseTransformPoint(grapplePoint);
		springJoint.enabled = false;

		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, transform.position);
		StartCoroutine(EnableJoint(springJoint));
		StartCoroutine(LerpRope(grapplePoint));
		lineRenderer.enabled = true;
		grappling = true;
	}

	void EndGrapple()
	{
		canGrapple = false;

		Destroy(gameObject.GetComponent<SpringJoint2D>());
		lineRenderer.enabled = false;
		grappling = false;

		canGrapple = true;
	}

	IEnumerator LerpRope(Vector2 target)
	{
		canGrapple = false;

		float time = 0;
		while (time < ropeLerpTime)
		{
			lineRenderer.SetPosition(1, Vector2.Lerp(transform.position, target, time / ropeLerpTime));
			time += Time.fixedDeltaTime;
			yield return null;
		}
		lineRenderer.SetPosition(1, target);

		canGrapple = true;
	}

	IEnumerator EnableJoint(Joint2D joint)
	{
		canGrapple = false;

		yield return new WaitForSeconds(ropeLerpTime);
		joint.enabled = true;

		canGrapple = true;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(transform.position, targetDistance);
	}
}