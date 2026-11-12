using NGame2.NBattleView;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NRender;

public static class NodeTree_ISceneContainer_ExtensionMethod
{
	public static ICamera GETCAMERA(this ISceneContainer This)
	{
		return This?.GetCamera();
	}

	public static RenderFacade GETRENDERFACADE(this ISceneContainer This, int uid)
	{
		return This?.GetRenderFacade(uid);
	}

	public static Creature GETBATTLECREATURE(this ISceneContainer This, int uid)
	{
		return This?.GetBattleCreature(uid);
	}
}
