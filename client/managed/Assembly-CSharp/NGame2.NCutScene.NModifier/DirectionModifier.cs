using LitJson;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

public class DirectionModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public float EndAngle;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private float _beginAngle;

	public bool IsEnd { get; private set; }

	public void Begin(CreatureModifier creatureMod)
	{
		if (!IsEnable || creatureMod == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_beginAngle = creatureMod.Angle;
	}

	public void Update(CreatureModifier creatureMod, int elapsedTick)
	{
		if (IsEnable)
		{
			if (BeginTick == EndTick)
			{
				creatureMod.Angle = EndAngle;
			}
			else
			{
				float num = Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				creatureMod.Angle = Mathf.Lerp(_beginAngle, EndAngle, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num));
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
			creatureMod.Angle = EndAngle;
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		EndAngle = json["EndAngle"].GETFLOAT(EndAngle);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldDouble("EndAngle", EndAngle);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
