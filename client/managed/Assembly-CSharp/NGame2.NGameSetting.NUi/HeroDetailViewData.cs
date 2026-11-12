using LitJson;

namespace NGame2.NGameSetting.NUi;

public class HeroDetailViewData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public float FieldOfView = 25f;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		FieldOfView = json["FieldOfView"].GETFLOAT(FieldOfView);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("FieldOfView", FieldOfView);
		return jsonData;
	}
}
