using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowControls : MonoBehaviour
{
    [SerializeField] GameObject controls;
	private void Update()
	{
		if (Input.GetButtonDown("Controls"))
			controls.SetActive(!controls.activeSelf);
	}
}
