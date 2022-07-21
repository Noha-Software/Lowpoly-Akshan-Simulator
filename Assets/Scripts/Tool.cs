using UnityEngine;

public abstract class Tool : MonoBehaviour
{
	public abstract string Name { get; }
	public abstract string Description { get; }
	public abstract Sprite Texture { get; }

	public abstract void Use();
}