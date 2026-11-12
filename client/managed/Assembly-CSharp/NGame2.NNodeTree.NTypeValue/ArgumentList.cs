using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue;

public class ArgumentList : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	private List<ArgumentSingle> _list = new List<ArgumentSingle>();

	public int Count => _list.Count;

	public ArgumentSingle this[int index] => Vespa_Generic_IList_Extension_Method.GetValid(_list, index);

	public void Wakeup(IVariable variable)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_list); i++)
		{
			_list[i].GetObjectValue(variable);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsArray())
		{
			return false;
		}
		for (int i = 0; i < json.Count; i++)
		{
			ArgumentSingle argumentSingle = new ArgumentSingle();
			argumentSingle.LoadFromJson(json.GetFromArray(i));
			_list.Add(argumentSingle);
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		for (int i = 0; i < _list.Count; i++)
		{
			jsonData.Add(_list[i].MakeJson());
		}
		return jsonData;
	}
}
