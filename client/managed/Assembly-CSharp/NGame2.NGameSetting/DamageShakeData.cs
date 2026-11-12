using LitJson;

namespace NGame2.NGameSetting;

public class DamageShakeData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public float TimeLength = 0.1f;

	public float MidFrame = 0.3f;

	public float Distance = 0.1f;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		TimeLength = json.GetJsonAny("TimeLength", "time").GETFLOAT(TimeLength);
		MidFrame = json.GetJsonAny("MidFrame", "mid_frame").GETFLOAT(MidFrame);
		Distance = json.GetJsonAny("Distance", "distance").GETFLOAT(Distance);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("TimeLength", TimeLength);
		jsonData.AddFieldDouble("MidFrame", MidFrame);
		jsonData.AddFieldDouble("Distance", Distance);
		return jsonData;
	}
}
