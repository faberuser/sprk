using LitJson;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue;

public class ArgumentSingle
{
	public ArgumentHolder Holder { get; private set; }

	public ArgumentSingle()
	{
		Holder = new ArgumentHolder();
	}

	public ArgumentSingle(params string[] DefaultKeyword)
	{
		Holder = new ArgumentHolder(DefaultKeyword);
	}

	public object GetObjectValue(IVariable variable)
	{
		if (Holder.Value == null)
		{
			return null;
		}
		return Holder.Value.GetObjectValue(variable);
	}

	public bool LoadFromJson(JsonData json)
	{
		return Holder.LOADFROMJSON(json);
	}

	public JsonData MakeJson()
	{
		return Holder.MAKEJSON();
	}
}
