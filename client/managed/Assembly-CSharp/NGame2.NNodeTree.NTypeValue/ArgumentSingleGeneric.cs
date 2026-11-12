using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue;

public class ArgumentSingleGeneric<T> : ArgumentSingle
{
	public ArgumentSingleGeneric(T value)
	{
	}

	public ArgumentSingleGeneric(StringKey variableKey)
	{
	}

	public T GetValue(IVariable variable)
	{
		if (!TryGetValue(variable, out var value))
		{
			return default(T);
		}
		return value;
	}

	public T GetValue(IVariable variable, T defaultValue)
	{
		if (!TryGetValue(variable, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public bool TryGetValue(IVariable variable, out T value)
	{
		ITypeValue<T> typeValue = GetTypeValue();
		if (typeValue == null)
		{
			value = default(T);
			return true;
		}
		bool flag = typeValue.TryGetValue(variable, out value);
		if (!flag)
		{
			value = default(T);
			return true;
		}
		return flag;
	}

	public ITypeValue<T> GetTypeValue()
	{
		return base.Holder.Value.Casting<T>();
	}

	private string GetClassTypeName()
	{
		return $"ArgumentSingleGeneric<{typeof(T).Name}>";
	}

	private string GetHolderTypeName()
	{
		if (base.Holder == null || base.Holder.Value == null)
		{
			return "null";
		}
		return base.Holder.Value.GetValueType().Name;
	}
}
