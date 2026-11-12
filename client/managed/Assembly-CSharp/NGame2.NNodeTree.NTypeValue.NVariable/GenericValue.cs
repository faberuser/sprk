using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue.NVariable;

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
		IVariableValue<T> variableValue = variable.GetValueBase(base.Name).Casting<T>();
		if (variableValue == null)
		{
			o_value = default(T);
			return true;
		}
		o_value = variableValue.GetValue();
		return true;
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new GenericValue<T>(this);
	}
}
