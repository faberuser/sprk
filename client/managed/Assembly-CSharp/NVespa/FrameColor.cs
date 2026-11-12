using System;
using LitJson;
using UnityEngine;

namespace NVespa;

[Serializable]
public class FrameColor : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	private const string JsonPrefix = "[FrameColor:Json]";

	public AnimationCurve CurveFrame = new AnimationCurve();

	public Gradient ColorGradient = new Gradient();

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		JsonData jsonAny = json.GetJsonAny("CurveFrame", "curve_frame");
		JsonData jsonAny2 = json.GetJsonAny("Gradient", "gradient");
		CurveFrame = JsonTools.ToCurve(jsonAny, CurveFrame);
		ColorGradient = JsonTools.ToGradient(jsonAny2, ColorGradient);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("CurveFrame", JsonTools.FromCurve(CurveFrame));
		jsonData.AddField("Gradient", JsonTools.FromGradient(ColorGradient));
		return jsonData;
	}

	public Color Evaluate(float frame)
	{
		if (!TryEvaluate(frame, out var o_value))
		{
			return Color.white;
		}
		return o_value;
	}

	public bool TryEvaluate(float frame, out Color o_value)
	{
		if (ColorGradient == null)
		{
			o_value = Color.white;
			return false;
		}
		float time = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(CurveFrame, frame, frame);
		o_value = ColorGradient.Evaluate(time);
		return true;
	}
}
