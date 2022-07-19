using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : NetworkBehaviour
{
    Camera cam;

	private void Awake()
	{
        cam = Camera.main;
	}

	void FixedUpdate()
    {
        Follow();
    }

    [Client(RequireOwnership = true)]
    void Follow()
    {
        Vector3 pos = cam.transform.position;
        pos.x = transform.position.x;
        pos.y = transform.position.y;
        cam.transform.position = pos;
    }
}
