using LitJson;
using NGameAsset;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

public class BgmVolumeModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable = true;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve VolumeCurve = new AnimationCurve();

	public bool IsEnd { get; private set; }

	public void Begin()
	{
		if (!IsEnable)
		{
			IsEnd = true;
			return;
		}
		float volume = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(VolumeCurve, 0f, 0f);
		SingletonAuto<BgmManager2>.instance.SetVolume(volume);
		IsEnd = false;
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd)
		{
			if (BeginTick < EndTick)
			{
				float num = Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float volume = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(VolumeCurve, num, num);
				SingletonAuto<BgmManager2>.instance.SetVolume(volume);
			}
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable)
		{
			float volume = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(VolumeCurve, 1f, 1f);
			SingletonAuto<BgmManager2>.instance.SetVolume(volume);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		VolumeCurve = json["VolumeCurve"].GetAnimationCurve(VolumeCurve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddFieldAnimationCurve("VolumeCurve", VolumeCurve);
		return jsonData;
	}
}
