using LitJson;

namespace NGame2.NGameSetting;

public class VolumeData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	bool ILoadJson.Load(JsonData json)
	{
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		return new JsonData();
	}
}
