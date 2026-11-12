using System.Collections.Generic;
using LitJson;

namespace NGame2.NGameSetting.NGameAssetData;

public class PreloadData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public List<string> NodeTreeList = new List<string>();

	public List<string> EffectCodeList = new List<string>();

	public List<string> SoundCodeList = new List<string>();

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		NodeTreeList.LoadFromJsonEx(json["NodeTreeList"]);
		EffectCodeList.LoadFromJsonEx(json["EffectCodeList"]);
		SoundCodeList.LoadFromJsonEx(json["SoundCodeList"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("NodeTreeList", Json_Extension_Method_IList.MakeJson(NodeTreeList));
		jsonData.AddField("EffectCodeList", Json_Extension_Method_IList.MakeJson(EffectCodeList));
		jsonData.AddField("SoundCodeList", Json_Extension_Method_IList.MakeJson(SoundCodeList));
		return jsonData;
	}
}
