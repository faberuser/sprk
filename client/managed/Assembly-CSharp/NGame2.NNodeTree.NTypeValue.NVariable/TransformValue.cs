using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NRender;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NVariable;

public class TransformValue : ValueBase<Transform>
{
	public TransformValue()
	{
	}

	public TransformValue(TransformValue prototype)
		: base((ValueBase<Transform>)prototype)
	{
	}

	protected override bool TryGetValueDerived(IVariable variable, out Transform o_value)
	{
		IVariableValueBase valueBase = variable.GetValueBase(base.Name);
		IVariableValue<Transform> variableValue = valueBase.Casting<Transform>();
		if (variableValue != null)
		{
			o_value = variableValue.GetValue();
			return true;
		}
		IVariableValue<long> variableValue2 = valueBase.Casting<long>();
		if (variableValue2 != null)
		{
			o_value = GetTransformFromUid(variable, variableValue2.GetValue());
			return o_value != null;
		}
		IVariableValue<object> variableValue3 = valueBase.Casting<object>();
		if (variableValue3 != null)
		{
			Component component = variableValue3.GetValue() as Component;
			o_value = ((component == null) ? null : component.transform);
			return o_value != null;
		}
		o_value = null;
		return false;
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new TransformValue(this);
	}

	private Transform GetTransformFromUid(IVariable variable, long uid)
	{
		RenderFacade rENDERFACADE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETRENDERFACADE((int)uid);
		if (rENDERFACADE != null)
		{
			return rENDERFACADE.transform;
		}
		ObjectBase objectBase = Container.instance.GetObjectBase(uid);
		if (objectBase != null)
		{
			return objectBase.transform;
		}
		return null;
	}
}
