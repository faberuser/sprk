using LitJson;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NConstant;

public class Vector3Value : AbstractValue<Vector3>
{
	private Vector3 _value;

	public override Vector3 Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public Vector3Value()
	{
	}

	public Vector3Value(Vector3Value prototype)
	{
		Value = prototype.Value;
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new Vector3Value(this);
	}

	protected override bool LoadFromBodyJsonDerived(JsonData json)
	{
		Value = JsonTools.ToVector3(json, Value);
		return true;
	}

	protected override JsonData MakeBodyJsonDerived()
	{
		return JsonTools.FromVector3(Value);
	}
}
