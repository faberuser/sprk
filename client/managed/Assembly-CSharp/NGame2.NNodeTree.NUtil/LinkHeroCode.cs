using LitJson;
using NShared;

namespace NGame2.NNodeTree.NUtil;

public class LinkHeroCode : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	private static CreatureData[] CreatureAllData;

	public string Code = string.Empty;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		Code = json["Code"].GETSTRING(Code);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Code", Code);
		return jsonData;
	}
}
