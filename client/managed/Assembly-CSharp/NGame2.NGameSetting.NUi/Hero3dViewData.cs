using LitJson;

namespace NGame2.NGameSetting.NUi;

public class Hero3dViewData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public float CameraAngle = 5f;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		CameraAngle = json["CameraAngle"].GETFLOAT(CameraAngle);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("CameraAngle", CameraAngle);
		return jsonData;
	}
}
