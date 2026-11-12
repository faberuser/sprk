using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue.NSelect;

public class GenericValue<T> : ValueBase<T>
{
	public GenericValue()
	{
	}

	public GenericValue(GenericValue<T> prototype)
		: base((ValueBase<T>)prototype)
	{
	}

	protected override bool TryGetValueDerived(IVariable variable, out T o_value)
	{
		bool flag = true;
		ITypeValue<bool> typeValue = _argumentContainer.GetTypeValue<bool>(0);
		if (typeValue != null)
		{
			flag = typeValue.GetValue(variable);
		}
		ITypeValue<T> typeValue2 = ((!flag) ? _argumentContainer.GetTypeValue<T>(2) : _argumentContainer.GetTypeValue<T>(1));
		if (typeValue2 == null)
		{
			o_value = default(T);
			return true;
		}
		return typeValue2.TryGetValue(variable, out o_value);
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new GenericValue<T>(this);
	}
}
