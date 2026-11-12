using LitJson;
using UnityEngine;

namespace NGame2.NGameSetting;

public class TutorialData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public Color EmptyColor = new Color(0f, 0f, 0f, 0f);

	public Color DarkColor = new Color(0f, 0f, 0f, 0.7f);

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		EmptyColor = json["EmptyColor"].GETCOLOR(EmptyColor);
		DarkColor = json["DarkColor"].GETCOLOR(DarkColor);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldColor("EmptyColor", EmptyColor);
		jsonData.AddFieldColor("DarkColor", DarkColor);
		return jsonData;
	}
}
