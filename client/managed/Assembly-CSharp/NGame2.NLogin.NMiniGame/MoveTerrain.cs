using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

internal class MoveTerrain : IGameEvent
{
	public int TerrainID;

	public Vector2 Pos;

	public virtual void Set(int id, Vector2 pos)
	{
		TerrainID = id;
		Pos = pos;
	}

	public virtual void Reset()
	{
		TerrainID = 0;
		Pos = Vector2.zero;
	}
}
