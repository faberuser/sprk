using NGame2.NNodeTree;
using NGame2.NNodeTree.NRuntime;

public static class NodeTree_Runtime_IVariable_ExtensionMethod
{
	public static RuntimeInstance RUNTIMEINSTANCE(this IVariable This)
	{
		return This?.GetRuntimeInstance();
	}
}
