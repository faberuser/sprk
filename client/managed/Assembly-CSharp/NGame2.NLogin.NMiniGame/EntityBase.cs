using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

public class EntityBase
{
	public Rect Rect;

	public IGameGlobal GameGlobal { get; protected set; }

	public int Id { get; private set; }

	public void SetId(int id)
	{
		Id = id;
	}

	public EntityBase(IGameGlobal gameGlobal)
	{
		GameGlobal = gameGlobal;
	}

	public EntityBase(IGameGlobal gameGlobal, int id)
	{
		GameGlobal = gameGlobal;
		Id = id;
	}
}
