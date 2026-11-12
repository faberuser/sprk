using JsonBin;
using UnityEngine;

namespace NGameAsset.NEffect;

public class EndingDisappearData : ILoadJson
{
	public bool EnableThis;

	public float TimeLength = 1f;

	public AnimationCurve AlphaCurve = new AnimationCurve();

	bool ILoadJson.Load(JsonRead json)
	{
		EnableThis = json["EnableThis"].GETBOOLEAN(EnableThis);
		TimeLength = json["TimeLength"].GETFLOAT(TimeLength);
		AlphaCurve = json["AlphaCurve"].GetAnimationCurve(AlphaCurve);
		return true;
	}
}
