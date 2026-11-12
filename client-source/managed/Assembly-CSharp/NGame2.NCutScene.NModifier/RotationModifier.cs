using LitJson;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

public class RotationModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public Vector3 EndRotation;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private Vector3 _beginRotation;

	public bool IsEnd { get; private set; }

	public void Begin(CreatureModifier creatureMod)
	{
		if (!IsEnable || creatureMod == null)
		{
			IsEnd = true;
			return;
		}
		IsEnd = false;
		_beginRotation = creatureMod.Rotation;
	}

	public void Update(CreatureModifier creatureMod, int elapsedTick)
	{
		if (IsEnable)
		{
			if (BeginTick == EndTick)
			{
				creatureMod.Rotation = EndRotation;
			}
			else
			{
				float t = Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				creatureMod.Rotation = Vector3.Lerp(_beginRotation, EndRotation, t);
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
			creatureMod.Rotation = EndRotation;
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		EndRotation = json["EndRotation"].GETVECTOR3(EndRotation);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldVector3("EndRotation", EndRotation);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
