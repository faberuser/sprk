using LitJson;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

public class FacialAnimationModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public float StartTick;

	public string[] FacialNameArray = new string[2]
	{
		string.Empty,
		string.Empty
	};

	public bool BlendAction;

	public float BlendTime;

	public AnimationCurve BlendCurve;

	bool ILoadJson.Load(JsonData json)
	{
		StartTick = json["StartTick"].GETFLOAT(StartTick);
		FacialNameArray.LoadFromJsonEx(json["FacialNameArray"]);
		BlendAction = json["BlendAction"].GETBOOLEAN(BlendAction);
		BlendTime = json["BlendTime"].GETFLOAT(BlendTime);
		BlendCurve = json["BlendCurve"].GetAnimationCurve(BlendCurve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("StartTick", StartTick);
		jsonData.AddField("FacialNameArray", Json_Extension_Method_IList.MakeJson(FacialNameArray));
		jsonData.AddFieldBool("BlendAction", BlendAction);
		jsonData.AddFieldDouble("BlendTime", BlendTime);
		jsonData.AddFieldAnimationCurve("BlendCurve", BlendCurve);
		return jsonData;
	}
}
