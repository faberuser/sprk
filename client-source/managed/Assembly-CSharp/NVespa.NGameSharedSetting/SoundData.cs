using LitJson;
using UnityEngine;

namespace NVespa.NGameSharedSetting;

public class SoundData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public float MaxVolumeBlendTime = 0.2f;

	public float TimeForOverlapDetect = 0.05f;

	public AnimationCurve BgmFadeOutCurve = new AnimationCurve();

	public float BgmFadeOutTime = 0.5f;

	public AnimationCurve SkillSoundFadeOutCurve = new AnimationCurve();

	public float SkillSoundFadeOutTime = 0.5f;

	public int SkillSoundMaxCount = 3;

	public float SkillSoundDecreaseVolumeRate = 0.5f;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		MaxVolumeBlendTime = json["MaxVolumeBlendTime"].GETFLOAT(MaxVolumeBlendTime);
		TimeForOverlapDetect = json["TimeForOverlapDetect"].GETFLOAT(TimeForOverlapDetect);
		BgmFadeOutCurve = json["BgmFadeOutCurve"].GetAnimationCurve(BgmFadeOutCurve);
		BgmFadeOutTime = json["BgmFadeOutTime"].GETFLOAT(BgmFadeOutTime);
		SkillSoundFadeOutCurve = json["SkillSoundFadeOutCurve"].GetAnimationCurve(SkillSoundFadeOutCurve);
		SkillSoundFadeOutTime = json["SkillSoundFadeOutTime"].GETFLOAT(SkillSoundFadeOutTime);
		SkillSoundMaxCount = json["SkillSoundMaxCount"].GETINT(SkillSoundMaxCount);
		SkillSoundDecreaseVolumeRate = json["SkillSoundDecreaseVolumeRate"].GETFLOAT(SkillSoundDecreaseVolumeRate);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("MaxVolumeBlendTime", MaxVolumeBlendTime);
		jsonData.AddFieldDouble("TimeForOverlapDetect", TimeForOverlapDetect);
		jsonData.AddFieldAnimationCurve("BgmFadeOutCurve", BgmFadeOutCurve);
		jsonData.AddFieldDouble("BgmFadeOutTime", BgmFadeOutTime);
		jsonData.AddFieldAnimationCurve("SkillSoundFadeOutCurve", SkillSoundFadeOutCurve);
		jsonData.AddFieldDouble("SkillSoundFadeOutTime", SkillSoundFadeOutTime);
		jsonData.AddFieldLong("SkillSoundMaxCount", SkillSoundMaxCount);
		jsonData.AddFieldDouble("SkillSoundDecreaseVolumeRate", SkillSoundDecreaseVolumeRate);
		return jsonData;
	}
}
