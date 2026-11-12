using LitJson;

namespace NGame2.NNodeTree.NTypeValue.NConstant;

public class DecimalValue : AbstractValue<decimal>
{
	public override decimal Value { get; set; }

	public DecimalValue()
	{
		Value = 0m;
	}

	public DecimalValue(DecimalValue prototype)
	{
		Value = prototype.Value;
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new DecimalValue(this);
	}

	protected override bool LoadFromBodyJsonDerived(JsonData json)
	{
		Value = Vespa_String_Extension_Method.TODECIMAL(json.GETSTRING(string.Empty), Value);
		return true;
	}

	protected override JsonData MakeBodyJsonDerived()
	{
		return new JsonData(Value.ToString());
	}
}
