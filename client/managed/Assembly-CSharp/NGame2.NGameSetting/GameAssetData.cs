using LitJson;
using NGameAsset.NCreature;

namespace NGame2.NGameSetting;

public class GameAssetData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public AnimationBlendDefault AnimationBlend = new AnimationBlendDefault();

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		AnimationBlend.LOADFROMJSON(json["AnimationBlend"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("AnimationBlend", AnimationBlend);
		return jsonData;
	}
}
