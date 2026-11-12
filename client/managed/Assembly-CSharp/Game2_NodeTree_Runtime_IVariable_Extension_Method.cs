using NGame2.NBattleView;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NRuntime;

public static class Game2_NodeTree_Runtime_IVariable_Extension_Method
{
	public static Creature GetBattleCreature(this IVariable This, StringKey key)
	{
		if (This == null)
		{
			return null;
		}
		long value = This.GetValue<long>(key);
		if (value == 0L)
		{
			return null;
		}
		return This.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)value);
	}

	public static T GetValue<T>(this IVariable This, StringKey key)
	{
		if (This.TryGetValue<T>(key, out var o_value))
		{
			return o_value;
		}
		return default(T);
	}

	public static bool TryGetValue<T>(this IVariable This, StringKey key, out T o_value)
	{
		if (This == null)
		{
			o_value = default(T);
			return false;
		}
		IVariableValue<T> variableValue = This.GetValueBase(key).Casting<T>();
		if (variableValue == null)
		{
			o_value = default(T);
			return false;
		}
		o_value = variableValue.GetValue();
		return true;
	}
}
