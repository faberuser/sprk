using LitJson;

namespace NGame2.NNodeTree.NTypeValue.NConstant;

public class FloatValue : AbstractValue<float>
{
	public override float Value { get; set; }

	public FloatValue()
	{
	}

	public FloatValue(FloatValue prototype)
	{
		Value = prototype.Value;
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new FloatValue(this);
	}

	protected override bool LoadFromBodyJsonDerived(JsonData json)
	{
		Value = json.GETFLOAT(Value);
		return true;
	}

	protected override JsonData MakeBodyJsonDerived()
	{
		return new JsonData(Value);
	}
}
