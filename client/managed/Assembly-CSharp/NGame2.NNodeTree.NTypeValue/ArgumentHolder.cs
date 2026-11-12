using LitJson;
using NVespa;

namespace NGame2.NNodeTree.NTypeValue;

public class ArgumentHolder : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public ITypeValueBase Value;

	public ArgumentHolder()
	{
		Value = SingletonAuto<ValueFactory>.instance.CreateDefault();
	}

	public ArgumentHolder(params string[] DefaultKeyword)
	{
		Value = SingletonAuto<ValueFactory>.instance.CreateByKeyword(DefaultKeyword);
	}

	public ArgumentHolder(ITypeValueBase value)
	{
		Value = value;
	}

	bool ILoadJson.Load(JsonData json)
	{
		ITypeValueBase typeValueBase = SingletonAuto<ValueFactory>.instance.CreateFromJson(json);
		if (typeValueBase != null)
		{
			Value = typeValueBase;
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		return SingletonAuto<ValueFactory>.instance.MakeJson(Value);
	}
}
