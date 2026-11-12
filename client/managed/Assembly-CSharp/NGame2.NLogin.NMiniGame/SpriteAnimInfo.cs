using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

public class SpriteAnimInfo
{
	public string SpriteName;

	public Vector3 PosAddVal;

	public SpriteAnimInfo(string name, Vector3 pos)
	{
		SpriteName = name;
		PosAddVal = pos;
	}
}
