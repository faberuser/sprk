using LitJson;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

public class ScaleModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public bool IsRelativeScale;

	public float ScaleValue;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve BlendCurve = new AnimationCurve();

	private float _beginScale;

	public bool IsEnd { get; private set; }

	public void Begin(CreatureModifier creatureMod)
	{
		if (!IsEnable || creatureMod == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_beginScale = creatureMod.Scale;
	}

	public void Update(CreatureModifier creatureMod, int elapsedTick)
	{
		if (IsEnable && !IsEnd)
		{
			float num = (IsRelativeScale ? (_beginScale * ScaleValue) : ScaleValue);
			if (BeginTick == EndTick)
			{
				creatureMod.Scale = num;
			}
			else
			{
				float num2 = Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				creatureMod.Scale = Mathf.Lerp(_beginScale, num, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(BlendCurve, num2, num2));
			}
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	public void Skip(CreatureModifier creatureMod)
	{
		IsEnd = true;
		if (IsEnable)
		{
			float scale = (IsRelativeScale ? (_beginScale * ScaleValue) : ScaleValue);
			creatureMod.Scale = scale;
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		IsRelativeScale = json["IsRelativeScale"].GETBOOLEAN(IsRelativeScale);
		ScaleValue = json["ScaleValue"].GETFLOAT(ScaleValue);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		BlendCurve = json["BlendCurve"].GetAnimationCurve(BlendCurve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldBool("IsRelativeScale", IsRelativeScale);
		jsonData.AddFieldDouble("ScaleValue", ScaleValue);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddFieldAnimationCurve("BlendCurve", BlendCurve);
		return jsonData;
	}
}
