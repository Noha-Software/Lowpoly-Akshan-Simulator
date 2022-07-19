using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class RopeManager : NetworkBehaviour
{
	public static RopeManager instance;
	[SerializeField] Material ropeMaterial;
	[SerializeField] List<RopeRenderer> ropes = new List<RopeRenderer>();

	private void Awake()
	{
		instance = this;
	}

	[ServerRpc(RequireOwnership = false)]
	public void AddRopeRequest(Vector3[] points, float width, NetworkConnection owner)
	{
		int i = AddRope(points, width, owner);
		//TODO: find way to get rope's index to player
	}
	[ServerRpc(RequireOwnership = false)]
	public void RemoveRopeRequest(int i)
	{
		RemoveRope(i);
	}
	[ServerRpc(RequireOwnership = true)]
	public void UpdatePointsRequest(int i, Vector3[] points)
	{
		UpdatePoints(i, points);
	}
	[ServerRpc(RequireOwnership = true)]
	public void UpdatePointRequest(int ropeIndex, int pointIndex, Vector3 point)
	{
		UpdatePoint(ropeIndex, pointIndex, point);
	}

	int AddRope(Vector3[] points, float width, NetworkConnection owner)
	{
		GameObject rope = new GameObject();
		GameObject ropeObj = Instantiate(rope, transform);
		RopeRenderer ropeRenderer = new RopeRenderer(points, ropeMaterial, width, ropeObj);
		ropeObj.AddComponent<NetworkObject>().GiveOwnership(owner);
		ropeObj.name = "rope_" + ropes.Count;
		ropes.Add(ropeRenderer);
		int index = -1;
		for (int i = 0; i < ropes.Count; i++)
		{
			if (ropes[i] == ropeRenderer) index = i; 
		}
		return index;
	}
	void RemoveRope(int i)
	{
		if (i >= ropes.Count || i < 0) return;
		Destroy(ropes[i].gameObject);
		ropes.RemoveAt(i);
	}
	void UpdatePoints(int i, Vector3[] points)
	{
		if (!base.IsServer) return;
		ropes[i].UpdatePoints(points);
	}
	void UpdatePoint(int ropeIndex, int pointIndex, Vector3 point)
	{
		if (!base.IsServer) return;
		ropes[ropeIndex].UpdatePoint(pointIndex, point);
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
	public void UpdatePoint(int i, Vector3 point)
	{
		if (!init) return;

		lr.SetPosition(i, point);
	}
}