using System;
using UnityEngine;

namespace NNguiExtension;

[Serializable]
public class UISpriteAnimInfo
{
	public string SpriteName;

	public Vector3 PositionAdd;

	public UISpriteAnimInfo(string name, Vector3 pos)
	{
		SpriteName = name;
		PositionAdd = pos;
	}
}
