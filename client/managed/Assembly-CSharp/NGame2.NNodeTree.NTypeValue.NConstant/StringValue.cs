using LitJson;

namespace NGame2.NNodeTree.NTypeValue.NConstant;

public class StringValue : AbstractValue<string>
{
	public override string Value { get; set; }

	public StringValue()
	{
		Value = string.Empty;
	}

	public StringValue(StringValue prototype)
	{
		Value = prototype.Value;
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new StringValue(this);
	}

	protected override bool LoadFromBodyJsonDerived(JsonData json)
	{
		Value = json.GETSTRING(Value);
		return true;
	}

	protected override JsonData MakeBodyJsonDerived()
	{
		return new JsonData(Value);
	}
}
