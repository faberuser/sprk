using NGame2.NNodeTree;

public static class NodeTree_RuntimeInstance_ExtensionMethod
{
	public static ISceneContainer SCENECONTAINER(this RuntimeInstance This)
	{
		return This?.SceneContainer;
	}
}
