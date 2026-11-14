using System.Collections.Generic;
using JsonBin;
using UnityEngine;

namespace NGameAsset;

// Camera paths SPRK adds for heroes the game never finished. Each copies another hero's camera and
// moves its whole path (position and target) by a constant offset in the hero's local space, so the
// shot keeps its motion but frames a differently sized or placed hero. See docs/heroes/maya-art.md.
public static class SprkDerivedCameras
{
	private static readonly Dictionary<string, (string Source, Vector3 Offset)> Derived = new Dictionary<string, (string, Vector3)>
	{
		// Maya's recruit shot: Selene's Get camera, shifted so Maya ends centred beside the NEW HERO panel.
		{ "Get.Maya", ("Get.Selene", new Vector3(0.3f, 0f, 0f)) }
	};

	public static CameraData Build(string code, JsonRead root)
	{
		if (code == null || !Derived.TryGetValue(code, out var derived))
		{
			return null;
		}
		JsonRead source = root[derived.Source];
		if (!source.IsObject())
		{
			return null;
		}
		CameraData data = new CameraData();
		data.LOADFROMJSON(source);
		data.Code = code;
		Shift(data.PositionCurve, derived.Offset);
		Shift(data.TargetCurve, derived.Offset);
		return data;
	}

	private static void Shift(CameraData.Vector3Curve curve, Vector3 offset)
	{
		curve.CurveX = Shift(curve.CurveX, offset.x);
		curve.CurveY = Shift(curve.CurveY, offset.y);
		curve.CurveZ = Shift(curve.CurveZ, offset.z);
	}

	private static AnimationCurve Shift(AnimationCurve curve, float offset)
	{
		if (curve == null || offset == 0f)
		{
			return curve;
		}
		Keyframe[] keys = curve.keys;
		if (keys.Length == 0)
		{
			// An empty curve evaluates to 0; give it the offset as a constant.
			return AnimationCurve.Constant(0f, 1f, offset);
		}
		for (int i = 0; i < keys.Length; i++)
		{
			keys[i].value += offset;
		}
		AnimationCurve shifted = new AnimationCurve(keys);
		shifted.preWrapMode = curve.preWrapMode;
		shifted.postWrapMode = curve.postWrapMode;
		return shifted;
	}
}
