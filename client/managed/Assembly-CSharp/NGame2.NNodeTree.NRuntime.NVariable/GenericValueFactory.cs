using System;
using LitJson;
using NVespa;

namespace NGame2.NNodeTree.NRuntime.NVariable;

public class GenericValueFactory<T> : IValueFactory
{
	public class ValueDynamicCalculate : IVariableValue<T>, IVariableValueBase
	{
		private ITypeValue<T> _typeValue;

		private IVariable _variable;

		public static ValueDynamicCalculate Create(ITypeValue<T> typeValue, IVariable variable)
		{
			ValueDynamicCalculate valueDynamicCalculate = ReservePoolUtil.Create<ValueDynamicCalculate>();
			valueDynamicCalculate._typeValue = typeValue;
			valueDynamicCalculate._variable = variable;
			return valueDynamicCalculate;
		}

		T IVariableValue<T>.GetValue()
		{
			return _typeValue.GetValue(_variable);
		}

		Type IVariableValueBase.GetValueType()
		{
			return typeof(T);
		}

		JsonData IVariableValueBase.DumpJson()
		{
			return new JsonData(_typeValue.GetValue(_variable).ToString() + "(dynamic)");
		}
	}

	public GenericValueFactory()
	{
		SingletonAuto<ReservePool<ValueDynamicCalculate>>.instance.ReserveByCount(1000);
		SingletonAuto<ReservePool<ValueGenericMember<T>>>.instance.ReserveByCount(1000);
	}

	IVariableValueBase IValueFactory.CreateValue(ITypeValueBase typeValueBase, IVariable variable, bool isDynamic)
	{
		if (typeValueBase == null)
		{
			return null;
		}
		ITypeValue<T> typeValue = typeValueBase.Casting<T>();
		if (typeValue == null)
		{
			return null;
		}
		if (isDynamic)
		{
			return ValueDynamicCalculate.Create(typeValue, variable);
		}
		return ValueGenericMember<T>.Create(typeValue.GetValue(variable));
	}
}
