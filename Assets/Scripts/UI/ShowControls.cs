using UnityEngine;

namespace Kevlaris.UI
{
	public class ShowControls : MonoBehaviour
	{
		[SerializeField] GameObject controls;
		private void Update()
		{
			if (Input.GetButtonDown("Controls"))
				controls.SetActive(!controls.activeSelf);
		}
	}
}