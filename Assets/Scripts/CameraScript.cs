using UnityEngine;

public class CameraScript : MonoBehaviour
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

    void Follow()
    {
        Vector3 pos = cam.transform.position;
        pos.x = transform.position.x;
        pos.y = transform.position.y;
        cam.transform.position = pos;
    }
}
