using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : NetworkBehaviour
{
	//[SerializeField] LineRenderer lineRenderer;
	bool mouseDown;
	[HideInInspector] public bool grappling = false;
	PlayerController player;

	[Header("Targeting")]
	[SerializeField] bool targetAll = false;
	[SerializeField] LayerMask targetLayer;
	[SerializeField] [Min(1)] [Tooltip("Maximum distance to grapple")] int targetDistance = 15;

	[Header("Rope")]
	[SerializeField] [Range(0,1)] [Tooltip("Length of rope compared to distance to grapple point")] float length = .7f;
	[SerializeField] bool launch = false;
	[SerializeField] [Min(0)] float ropeLerpTime = .3f;
	Vector3 grapplePoint;
	int ropeIndex;
	Camera cam;

	private void Awake()
	{
		cam = Camera.main;
		//lineRenderer.positionCount = 2;
		player = GetComponent<PlayerController>();
	}

	private void FixedUpdate()
	{
		mouseDown = Input.GetMouseButtonDown(0);
		if (mouseDown)
		{
			if (!grappling)
			{
				StartGrapple();
			}
			else
			{
				EndGrapple();
			}
		}
	}

	private void LateUpdate()
	{
		//lineRenderer.SetPosition(0, transform.position);
		RopeManager.instance.UpdatePointRequest(ropeIndex, 0, transform.position);
	}

	[TargetRpc]
	public void SetRopeIndex(NetworkConnection conn, int i)
	{
		ropeIndex = i;
	}

	[Client(RequireOwnership = true)]
	void StartGrapple()
	{
		Vector3 screenPoint = Input.mousePosition;
		screenPoint.z = transform.position.z;
		Vector3 worldPoint = cam.ScreenToWorldPoint(screenPoint);
		Vector3 origin = transform.position;
		Vector3 direction = player.arrow.GetChild(0).position - origin;

		RaycastHit2D hit;
		if (targetAll)
		{
			hit = Physics2D.Raycast(origin, direction, targetDistance);
		}
		else
		{
			hit = Physics2D.Raycast(origin, direction, targetDistance, targetLayer);
		}

		if (!hit) return;

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

		Vector3[] points = new Vector3[2];
		points[0] = transform.position;
		points[1] = transform.position;
		int i = RopeManager.instance.AddRopeRequest(points, .2f, base.Owner);
		StartCoroutine(EnableJoint(springJoint));
		StartCoroutine(LerpRope(grapplePoint, i));
	}

	[Client(RequireOwnership = true)]
	void EndGrapple()
	{
		Destroy(gameObject.GetComponent<SpringJoint2D>());
		//lineRenderer.enabled = false;
		RopeManager.instance.RemoveRopeRequest(ropeIndex);
		grappling = false;
	}

	IEnumerator LerpRope(Vector2 target, int i)
	{
		ropeIndex = i;
		Vector3[] points = new Vector3[2];
		points[0] = transform.position;
		float time = 0;
		while (time < ropeLerpTime)
		{
			points[1] = Vector2.Lerp(transform.position, target, time / ropeLerpTime);
			RopeManager.instance.UpdatePointsRequest(i, points);

			//lineRenderer.SetPosition(1, Vector2.Lerp(transform.position, target, time / ropeLerpTime));
			time += Time.fixedDeltaTime;
			yield return null;
		}
		//lineRenderer.SetPosition(1, target);
		points[1] = target;
		RopeManager.instance.UpdatePointsRequest(i, points);
	}

	IEnumerator EnableJoint(Joint2D joint)
	{
		yield return new WaitForSeconds(ropeLerpTime);
		joint.enabled = true;
		grappling = true;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(transform.position, targetDistance);
	}
}