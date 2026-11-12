using LitJson;

namespace NGame2.NGameSetting;

public class BattleFieldData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public float DropItem_BossExtraWeight = 2f;

	public float WaveMove_MoveSpeed = 5f;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		DropItem_BossExtraWeight = json["DropItem_BossExtraWeight"].GETFLOAT(DropItem_BossExtraWeight);
		WaveMove_MoveSpeed = json["WaveMove_CreatureMoveSpeed"].GETFLOAT(WaveMove_MoveSpeed);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("DropItem_BossExtraWeight", DropItem_BossExtraWeight);
		jsonData.AddFieldDouble("WaveMove_CreatureMoveSpeed", WaveMove_MoveSpeed);
		return jsonData;
	}
}
