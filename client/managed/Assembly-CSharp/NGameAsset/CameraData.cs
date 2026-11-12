using System;
using JsonBin;
using UnityEngine;

namespace NGameAsset;

[Serializable]
public class CameraData : ILoadJson
{
	[Serializable]
	public class Vector3Curve : ILoadJson
	{
		public AnimationCurve CurveX = new AnimationCurve();

		public AnimationCurve CurveY = new AnimationCurve();

		public AnimationCurve CurveZ = new AnimationCurve();

		public Vector3 Evaluate(float frame)
		{
			return new Vector3(Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(CurveX, frame, 0f), Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(CurveY, frame, 0f), Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(CurveZ, frame, 0f));
		}

		bool ILoadJson.Load(JsonRead json)
		{
			CurveX = json["CurveX"].GetAnimationCurve(CurveX);
			CurveY = json["CurveY"].GetAnimationCurve(CurveY);
			CurveZ = json["CurveZ"].GetAnimationCurve(CurveZ);
			return true;
		}
	}

	public string Code = string.Empty;

	public Vector3Curve PositionCurve = new Vector3Curve();

	public Vector3Curve TargetCurve = new Vector3Curve();

	public AnimationCurve OrthoCurve = new AnimationCurve();

	public AnimationCurve RollAngle = new AnimationCurve();

	bool ILoadJson.Load(JsonRead json)
	{
		Code = json["Code"].GETSTRING(Code);
		PositionCurve.LOADFROMJSON(json["PositionCurve"]);
		TargetCurve.LOADFROMJSON(json["TargetCurve"]);
		OrthoCurve = json["OrthoCurve"].GetAnimationCurve(OrthoCurve);
		RollAngle = json["RollAngle"].GetAnimationCurve(RollAngle);
		return true;
	}
}
