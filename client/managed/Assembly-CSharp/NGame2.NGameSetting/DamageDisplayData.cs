using LitJson;

namespace NGame2.NGameSetting;

public class DamageDisplayData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool Disabled;

	public float PopupTime = 0.1f;

	public float WaitTime = 0.5f;

	public float ShrinkTime = 0.3f;

	public float MoveUpTime = 0.5f;

	public float DisappearTime = 0.5f;

	public float MoveUpSpeed = 1f;

	public float WorldMapMoveUpSpeed = 100f;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		Disabled = json["Disabled"].GETBOOLEAN(Disabled);
		PopupTime = json.GetJsonAny("PopupTime", "popup_time").GETFLOAT(PopupTime);
		WaitTime = json.GetJsonAny("WaitTime", "wait_time").GETFLOAT(WaitTime);
		ShrinkTime = json.GetJsonAny("ShrinkTime", "shrink_time").GETFLOAT(ShrinkTime);
		MoveUpTime = json.GetJsonAny("MoveUpTime", "moveup_time").GETFLOAT(MoveUpTime);
		DisappearTime = json.GetJsonAny("DisappearTime", "disappear_time").GETFLOAT(DisappearTime);
		MoveUpSpeed = json.GetJsonAny("MoveUpSpeed", "moveup_speed").GETFLOAT(MoveUpSpeed);
		WorldMapMoveUpSpeed = json["WorldMapMoveUpSpeed"].GETFLOAT(MoveUpSpeed);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("Disabled", Disabled);
		jsonData.AddFieldDouble("PopupTime", PopupTime);
		jsonData.AddFieldDouble("WaitTime", WaitTime);
		jsonData.AddFieldDouble("ShrinkTime", ShrinkTime);
		jsonData.AddFieldDouble("MoveUpTime", MoveUpTime);
		jsonData.AddFieldDouble("DisappearTime", DisappearTime);
		jsonData.AddFieldDouble("MoveUpSpeed", MoveUpSpeed);
		jsonData.AddFieldDouble("WorldMapMoveUpSpeed", WorldMapMoveUpSpeed);
		return jsonData;
	}
}
