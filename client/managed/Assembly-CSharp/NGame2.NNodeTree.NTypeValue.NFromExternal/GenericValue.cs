using NGame2.NBattleView;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue.NFromExternal;

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
		if (TryGetValueFromExternal(variable, out o_value))
		{
			return true;
		}
		if (TryGetValueIfNotExist(variable, out o_value))
		{
			return true;
		}
		o_value = default(T);
		return true;
	}

	private bool TryGetValueFromExternal(IVariable variable, out T o_value)
	{
		if (base.Name.IsNullOrEmpty())
		{
			o_value = default(T);
			return false;
		}
		ITypeValue<long> typeValue = _argumentContainer.GetTypeValue<long>(0);
		if (typeValue == null)
		{
			o_value = default(T);
			return false;
		}
		long value = typeValue.GetValue(variable);
		Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)value);
		if (bATTLECREATURE == null)
		{
			o_value = default(T);
			return false;
		}
		IVariableValue<T> variableValue = bATTLECREATURE.Variable.GetValueBase(base.Name).Casting<T>();
		if (variableValue == null)
		{
			o_value = default(T);
			return false;
		}
		o_value = variableValue.GetValue();
		return true;
	}

	private bool TryGetValueIfNotExist(IVariable variable, out T o_value)
	{
		ITypeValue<T> typeValue = _argumentContainer.GetTypeValue<T>(1);
		if (typeValue == null)
		{
			o_value = default(T);
			return false;
		}
		return typeValue.TryGetValue(variable, out o_value);
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new GenericValue<T>(this);
	}
}
