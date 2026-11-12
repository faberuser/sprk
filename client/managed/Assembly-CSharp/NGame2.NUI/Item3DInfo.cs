using NShared;
using UnityEngine;

namespace NGame2.NUI;

public class Item3DInfo
{
	public ItemType ItemType { get; set; }

	public float Item3DScale { get; set; }

	public string Item3DModelName { get; set; }

	public Vector3 Position { get; set; }

	public Vector3 Rotation { get; set; }

	public Item3DInfo(ItemType itemType, float item3DScale, string item3DModelName, Vector3 position, Vector3 rotation)
	{
		ItemType = itemType;
		Item3DScale = item3DScale;
		Item3DModelName = item3DModelName;
		Position = position;
		Rotation = rotation;
	}
}
