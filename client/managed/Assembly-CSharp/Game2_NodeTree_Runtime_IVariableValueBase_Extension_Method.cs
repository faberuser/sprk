using NGame2.NNodeTree.NRuntime;

public static class Game2_NodeTree_Runtime_IVariableValueBase_Extension_Method
{
	public static IVariableValue<T> Casting<T>(this IVariableValueBase This)
	{
		return This as IVariableValue<T>;
	}
}
