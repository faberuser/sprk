using System.Collections.Generic;
using JsonBin;

namespace NGameAsset.NBattleFieldMap2;

public class EffectFxGroupData : ILoadJson
{
	public string Prefix = string.Empty;

	public List<EffectFxData> EffectFxList = new List<EffectFxData>();

	bool ILoadJson.Load(JsonRead json)
	{
		Prefix = json["Prefix"].GETSTRING(Prefix);
		EffectFxList.LoadListFromJson(json["EffectFxList"]);
		return true;
	}
}
