using LitJson;

namespace NGame2.NNodeTree.NTypeValue.NConstant;

public class BooleanValue : AbstractValue<bool>
{
	public override bool Value { get; set; }

	public BooleanValue()
	{
	}

	public BooleanValue(BooleanValue prototype)
	{
		Value = prototype.Value;
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new BooleanValue(this);
	}

	protected override bool LoadFromBodyJsonDerived(JsonData json)
	{
		Value = json.GETBOOLEAN(Value);
		return true;
	}

	protected override JsonData MakeBodyJsonDerived()
	{
		return new JsonData(Value);
	}
}
