using NGame2.NBattleView;
using NGame2.NNodeTree.NRuntime;
using NGame2.NRender;

public static class NodeTree_Runtime_ItemBase_ExtensionMethod
{
	public static Creature GetBattleCreature(this ItemBase This, int uid)
	{
		return This?.Runtime.SCENECONTAINER().GETBATTLECREATURE(uid);
	}

	public static RenderFacade GetRenderFacade(this ItemBase This, int uid)
	{
		return This?.Runtime.SCENECONTAINER().GETRENDERFACADE(uid);
	}
}
