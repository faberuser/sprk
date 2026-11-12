using LitJson;
using UnityEngine;

namespace NGame2.NGameSetting;

public class CutSceneData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public Color ColorFocus_OtherColorDefault = new Color(0f, 0f, 0f, 0.5f);

	public float DisplayText_TextSpeed = 10f;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		ColorFocus_OtherColorDefault = json["ColorFocus_OtherColorDefault"].GETCOLOR(ColorFocus_OtherColorDefault);
		DisplayText_TextSpeed = json["DisplayText_TextSpeed"].GETFLOAT(DisplayText_TextSpeed);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldColor("ColorFocus_OtherColorDefault", ColorFocus_OtherColorDefault);
		jsonData.AddFieldDouble("DisplayText_TextSpeed", DisplayText_TextSpeed);
		return jsonData;
	}
}
