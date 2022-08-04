using Photon.Pun;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	[SerializeField] Camera playerCamera;
	[SerializeField] PhotonView playerView;

	private void Awake()
	{
		if (playerCamera == null)
			playerCamera = Camera.main;
		if (playerView == null)
			playerView = transform.GetComponent<PhotonView>();
	}

	void FixedUpdate()
	{
		if (!playerView.IsMine) return;
		Follow();
	}

	void Follow()
	{
		Vector3 pos = playerCamera.transform.position;
		pos.x = playerView.transform.position.x;
		pos.y = playerView.transform.position.y;
		playerCamera.transform.position = pos;
	}
}
