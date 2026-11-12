using LitJson;
using UnityEngine;

namespace NGame2.NGameSetting.NUi;

public class GetItemAnimationData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public float TimeAnimationBefore = 0.12f;

	public float TimeAnimationLength = 0.28f;

	public float TimeNext = 0.2f;

	public AnimationCurve AlphaCurve = new AnimationCurve();

	public AnimationCurve ScaleCurve = new AnimationCurve();

	public float TimeAnimationTotal => TimeAnimationBefore + TimeAnimationLength;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		TimeAnimationBefore = json["TimeAnimationBefore"].GETFLOAT(TimeAnimationBefore);
		TimeAnimationLength = json["TimeAnimationLength"].GETFLOAT(TimeAnimationLength);
		TimeNext = json["TimeNext"].GETFLOAT(TimeNext);
		AlphaCurve = json["AlphaCurve"].GetAnimationCurve(AlphaCurve);
		ScaleCurve = json["ScaleCurve"].GetAnimationCurve(ScaleCurve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("TimeAnimationBefore", TimeAnimationBefore);
		jsonData.AddFieldDouble("TimeAnimationLength", TimeAnimationLength);
		jsonData.AddFieldDouble("TimeNext", TimeNext);
		jsonData.AddFieldAnimationCurve("AlphaCurve", AlphaCurve);
		jsonData.AddFieldAnimationCurve("ScaleCurve", ScaleCurve);
		return jsonData;
	}
}
