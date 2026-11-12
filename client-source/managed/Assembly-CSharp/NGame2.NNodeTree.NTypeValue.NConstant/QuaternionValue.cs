using LitJson;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NConstant;

public class QuaternionValue : AbstractValue<Quaternion>
{
	private Vector3 _euler;

	public override Quaternion Value
	{
		get
		{
			return Quaternion.Euler(_euler);
		}
		set
		{
			_euler = value.eulerAngles;
		}
	}

	public QuaternionValue()
	{
	}

	public QuaternionValue(QuaternionValue prototype)
	{
		_euler = prototype._euler;
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new QuaternionValue(this);
	}

	protected override bool LoadFromBodyJsonDerived(JsonData json)
	{
		_euler = JsonTools.ToVector3(json, _euler);
		return true;
	}

	protected override JsonData MakeBodyJsonDerived()
	{
		return JsonTools.FromVector3(_euler);
	}
}
