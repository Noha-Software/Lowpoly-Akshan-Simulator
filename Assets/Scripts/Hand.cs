using UnityEngine;

public class Hand : MonoBehaviour
{
	public Tool currentTool;
	[SerializeField] Sprite arrowSprite;

	public void SetTool(System.Type toolType)
	{
		Destroy(currentTool);
		currentTool = (Tool)gameObject.AddComponent(toolType);
	}

	public void RemoveTool()
	{
		Destroy(currentTool);
		currentTool = null;
		transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = arrowSprite;
	}
}
