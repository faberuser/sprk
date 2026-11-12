using LitJson;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

public class PositionModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public bool UseEndPosX = true;

	public bool UseEndPosY;

	public bool UseEndPosZ;

	public Vector3 EndPos;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	private Vector3 _beginPos;

	public bool IsEnd { get; private set; }

	public void Begin(CreatureModifier creatureMod)
	{
		if (!IsEnable || creatureMod == null)
		{
			IsEnd = true;
			return;
		}
		_beginPos = creatureMod.Position;
		IsEnd = false;
	}

	public void Update(CreatureModifier creatureMod, int elapsedTick)
	{
		if (IsEnable && !IsEnd)
		{
			Vector3 position;
			if (BeginTick == EndTick)
			{
				position = EndPos;
			}
			else
			{
				float num = Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				position = Vector3.Lerp(_beginPos, EndPos, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, num, num));
			}
			if (!UseEndPosX)
			{
				position.x = creatureMod.Position.x;
			}
			if (!UseEndPosY)
			{
				position.y = creatureMod.Position.y;
			}
			if (!UseEndPosZ)
			{
				position.z = creatureMod.Position.z;
			}
			creatureMod.Position = position;
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
			Vector3 endPos = EndPos;
			if (!UseEndPosX)
			{
				endPos.x = creatureMod.Position.x;
			}
			if (!UseEndPosY)
			{
				endPos.y = creatureMod.Position.y;
			}
			if (!UseEndPosZ)
			{
				endPos.z = creatureMod.Position.z;
			}
			creatureMod.Position = endPos;
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		UseEndPosX = json["UseEndPosX"].GETBOOLEAN(UseEndPosX);
		UseEndPosY = json["UseEndPosY"].GETBOOLEAN(UseEndPosY);
		UseEndPosZ = json["UseEndPosZ"].GETBOOLEAN(UseEndPosZ);
		EndPos = JsonTools.ToVector3(json["EndPos"], EndPos);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldBool("UseEndPosX", UseEndPosX);
		jsonData.AddFieldBool("UseEndPosY", UseEndPosY);
		jsonData.AddFieldBool("UseEndPosZ", UseEndPosZ);
		jsonData.AddField("EndPos", JsonTools.FromVector3(EndPos));
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
