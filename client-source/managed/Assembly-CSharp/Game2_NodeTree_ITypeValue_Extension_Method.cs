using NGame2.NNodeTree;
using NGame2.NNodeTree.NRuntime;

public static class Game2_NodeTree_ITypeValue_Extension_Method
{
	public static ITypeValue<T> Casting<T>(this ITypeValueBase This)
	{
		return This as ITypeValue<T>;
	}

	public static string GetValuePath(this ITypeValueBase This)
	{
		return This.ValuePath;
	}

	public static bool TRYGETVALUE<T>(this ITypeValue<T> This, IVariable variable, out T o_value)
	{
		if (This == null)
		{
			o_value = default(T);
			return false;
		}
		return This.TryGetValue(variable, out o_value);
	}
}
