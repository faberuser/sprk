using LitJson;

namespace NGame2.NNodeTree.NTypeValue.NConstant;

public class UidValue : AbstractValue<long>
{
	public override long Value { get; set; }

	public UidValue()
	{
	}

	public UidValue(UidValue prototype)
	{
		Value = prototype.Value;
	}

	protected override ITypeValueBase CreateFromPrototypeDerived()
	{
		return new UidValue(this);
	}

	protected override bool LoadFromBodyJsonDerived(JsonData json)
	{
		Value = json.GETLONG(Value);
		return true;
	}

	protected override JsonData MakeBodyJsonDerived()
	{
		return new JsonData(Value);
	}
}
