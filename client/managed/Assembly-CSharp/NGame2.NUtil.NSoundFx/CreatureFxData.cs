using LitJson;
using NGameAsset.NCreature;
using NVespa;

namespace NGame2.NUtil.NSoundFx;

public class CreatureFxData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public string Code = string.Empty;

	public IncreaseDic<AnimationKeyData, AnimationFxData> AnimationDic = new IncreaseDic<AnimationKeyData, AnimationFxData>();

	public CreatureFxData()
	{
	}

	public CreatureFxData(string code)
	{
		Code = code;
	}

	bool ILoadJson.Load(JsonData json)
	{
		Code = json["Code"].GETSTRING(Code);
		AnimationDic.LoadFromJson(json["AnimationDic"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Code", Code);
		jsonData.AddField("AnimationDic", AnimationDic.MakeJson());
		return jsonData;
	}
}
