using LitJson;
using UnityEngine;

namespace NGame2.NGameSetting.NUi;

public class PartySettingData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public float EnterTime = 1f;

	public AnimationCurve EnterCurve = new AnimationCurve();

	public AnimationCurve StepAsideCurve = new AnimationCurve();

	public float DisappearTime = 1f;

	public AnimationCurve DisappearCurve = new AnimationCurve();

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		EnterTime = json["EnterTime"].GETFLOAT(EnterTime);
		EnterCurve = JsonTools.ToCurve(json["EnterCurve"], EnterCurve);
		StepAsideCurve = JsonTools.ToCurve(json["StepAsideCurve"], StepAsideCurve);
		DisappearTime = json["DisappearTime"].GETFLOAT(DisappearTime);
		DisappearCurve = JsonTools.ToCurve(json["DisappearCurve"], DisappearCurve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("EnterTime", EnterTime);
		jsonData.AddField("EnterCurve", JsonTools.FromCurve(EnterCurve));
		jsonData.AddField("StepAsideCurve", JsonTools.FromCurve(StepAsideCurve));
		jsonData.AddFieldDouble("DisappearTime", DisappearTime);
		jsonData.AddField("DisappearCurve", JsonTools.FromCurve(DisappearCurve));
		return jsonData;
	}
}
