using LitJson;
using UnityEngine;

namespace NVespa.NGameSharedSetting.NNgui;

public class GridData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public AnimationCurve AnimateCurve = new AnimationCurve();

	public float AnimateTime = 0.3f;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		AnimateCurve = json["AnimateCurve"].GetAnimationCurve(AnimateCurve);
		AnimateTime = json["AnimateTime"].GETFLOAT(AnimateTime);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldAnimationCurve("AnimateCurve", AnimateCurve);
		jsonData.AddFieldDouble("AnimateTime", AnimateTime);
		return jsonData;
	}
}
