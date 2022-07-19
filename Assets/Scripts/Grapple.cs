using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : NetworkBehaviour
{
	[SerializeField] LineRenderer lineRenderer;
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

	Camera cam;

	private void Awake()
	{
		cam = Camera.main;
		lineRenderer.positionCount = 2;
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
		lineRenderer.SetPosition(0, transform.position);
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
			springJoint.distance = Vector2.Distance(transform.position, hit.point) * length;
			springJoint.frequency = .8f;
			springJoint.dampingRatio = 0.2f;
		}
		springJoint.enableCollision = true;
		springJoint.connectedBody = hit.rigidbody;
		springJoint.connectedAnchor = hit.transform.InverseTransformPoint(hit.point);
		springJoint.enabled = false;

		//implement new rope renderer

		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, transform.position);
		StartCoroutine("EnableJoint", springJoint);
		StartCoroutine("LerpRope", hit.point);
		lineRenderer.enabled = true;
	}

	[Client(RequireOwnership = true)]
	void EndGrapple()
	{
		Destroy(gameObject.GetComponent<SpringJoint2D>());
		lineRenderer.enabled = false;

		grappling = false;
	}

	IEnumerator LerpRope(Vector2 target)
	{
		float time = 0;
		while (time < ropeLerpTime)
		{
			lineRenderer.SetPosition(1, Vector2.Lerp(transform.position, target, time / ropeLerpTime));
			time += Time.fixedDeltaTime;
			yield return null;
		}
		lineRenderer.SetPosition(1, target);
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

public class RopeManager : NetworkBehaviour
{
	[SerializeField] Material ropeMaterial;
	[SerializeField] List<RopeRenderer> ropes = new List<RopeRenderer>();
	public RopeRenderer AddRope(Vector3[] points, float width)
	{
		GameObject rope = new GameObject();
		GameObject ropeObj = Instantiate(rope, transform);
		RopeRenderer ropeRenderer = new RopeRenderer(points, ropeMaterial, width, ropeObj);
		ropeObj.AddComponent<NetworkObject>();
		ropeObj.name = "rope_" + ropes.Count;
		ropes.Add(ropeRenderer);
		return ropeRenderer;
	}
	public void RemoveRope(RopeRenderer rope)
	{
		if (!ropes.Contains(rope)) return;
		ropes.Remove(rope);
		Destroy(rope.gameObject);
	}
	public void RemoveRope(int i)
	{
		if (i >= ropes.Count) return;
		Destroy(ropes[i].gameObject);
		ropes.RemoveAt(i);
	}
}

public class RopeRenderer : MonoBehaviour
{
	LineRenderer lr;
	bool init;
	public RopeRenderer(Vector3[] points, Material material, float width, GameObject gameObject)
	{
		lr = gameObject.GetComponent<LineRenderer>();
		if (lr == null)
			lr = gameObject.AddComponent<LineRenderer>();

		lr.numCapVertices = 10;
		lr.numCornerVertices = 1;
		lr.useWorldSpace = false;
		lr.positionCount = points.Length;
		lr.SetPositions(points);
		lr.material = material;
		lr.startWidth = width;
		lr.endWidth = width;

		init = true;
	}
	public void UpdatePoints(Vector3[] points)
	{
		if (!init) return;

		lr.SetPositions(points);
	}
}