using System.Collections.Generic;
using LitJson;
using NGameAsset.NCreature;

namespace NGame2.NUtil.NSoundFx;

public class CreatureContainer : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public Dictionary<string, CreatureFxData> Dic = new Dictionary<string, CreatureFxData>();

	public List<FrameSoundData> Get(string creatureCode, AnimationKeyData animationKey)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(Dic, creatureCode, null)?.AnimationDic.GETVALUE(animationKey, null)?.FrameSoundList;
	}

	bool ILoadJson.Load(JsonData json)
	{
		Dic.LoadFromJsonK(json["Dic"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("Dic", Json_Extension_Method_IDictionary.MakeJson(Dic));
		return jsonData;
	}
}
