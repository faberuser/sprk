using LitJson;

namespace NGame2.NUtil.NSoundFx;

public class FrameSoundData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public float Frame;

	public string SoundCode = string.Empty;

	bool ILoadJson.Load(JsonData json)
	{
		Frame = json["Frame"].GETFLOAT(Frame);
		SoundCode = json["SoundCode"].GETSTRING(SoundCode);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("Frame", Frame);
		jsonData.AddFieldString("SoundCode", SoundCode);
		return jsonData;
	}
}
