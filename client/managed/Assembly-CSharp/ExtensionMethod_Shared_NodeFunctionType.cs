using NShared;

public static class ExtensionMethod_Shared_NodeFunctionType
{
	public static bool IsBattleFunction(this NodeFunctionType This)
	{
		if (This != NodeFunctionType.Battle)
		{
			return This == NodeFunctionType.BattleOnce;
		}
		return true;
	}

	public static bool IsSubStoryFunction(this NodeFunctionType This)
	{
		if (This != NodeFunctionType.SubStoryBattle && This != NodeFunctionType.SubStoryEvent)
		{
			return This == NodeFunctionType.SubStoryBoss;
		}
		return true;
	}

	public static bool IsOrdealArenaFuction(this NodeFunctionType This)
	{
		if (This != NodeFunctionType.OrdealArenaBattle)
		{
			return This == NodeFunctionType.OrdealArenaEvent;
		}
		return true;
	}
}
