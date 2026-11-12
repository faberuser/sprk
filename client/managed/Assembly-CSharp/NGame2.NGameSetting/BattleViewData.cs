using System.Collections.Generic;
using LitJson;
using NGame2.NGameSetting.NBattleView;
using NGameAsset.NBattleFieldMap2;
using UnityEngine;

namespace NGame2.NGameSetting;

public class BattleViewData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public class CustomFieldCameraData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public string Name = string.Empty;

		public FieldCameraData CameraData = new FieldCameraData();

		bool ILoadJson.Load(JsonData json)
		{
			Name = json["Name"].GETSTRING(Name);
			CameraData.LOADFROMJSON(json["CameraData"]);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("Name", Name);
			jsonData.AddFieldJsonMake("CameraData", CameraData);
			return jsonData;
		}
	}

	public DamageDotData DamageDot = new DamageDotData();

	public DamageFloatData DamageFloatData = new DamageFloatData();

	public DamageDotData DamageDotNew = new DamageDotData();

	public DamageFloatData DamageFloatDataNew = new DamageFloatData();

	public float BattleCamera_WaveMoveFrontSpece = 4f;

	public float BattleCamera_BoundsBlendSpeed = 5f;

	public AnimationCurve BattleCamera_WeightCurve = new AnimationCurve();

	public FieldCameraData BattleCamera = new FieldCameraData();

	public List<CustomFieldCameraData> CustomBattleCameraList = new List<CustomFieldCameraData>();

	public float Creature_DeadDisappearDelay = 1f;

	public float CcKnockback_MaxDuration = 0.5f;

	public AnimationCurve CcKnockback_PositionCurve = new AnimationCurve();

	public float CcDown_DownPositionDuration = 1f;

	public AnimationCurve CcDown_DownPositionCurve = new AnimationCurve();

	public float CcDown_GetupDuration = 1f;

	private Dictionary<string, FieldCameraData> _battleCameraDic;

	public FieldCameraData GetBattleCamera(string name)
	{
		if (String_Extension_Method.IsNullOrEmpty(name))
		{
			return BattleCamera;
		}
		if (_battleCameraDic == null)
		{
			_battleCameraDic = new Dictionary<string, FieldCameraData>();
			for (int i = 0; i < CustomBattleCameraList.Count; i++)
			{
				CustomFieldCameraData customFieldCameraData = CustomBattleCameraList[i];
				_battleCameraDic.Add(customFieldCameraData.Name, customFieldCameraData.CameraData);
			}
		}
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_battleCameraDic, name, BattleCamera);
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		DamageFloatData.LOADFROMJSON(json["DamageFloatData"]);
		DamageDot.LOADFROMJSON(json["DamageDot"]);
		DamageFloatDataNew.LOADFROMJSON(json["DamageFloatDataNew"]);
		DamageDotNew.LOADFROMJSON(json["DamageDotNew"]);
		BattleCamera_WaveMoveFrontSpece = json["BattleCamera_WaveMoveFrontSpece"].GETFLOAT(BattleCamera_WaveMoveFrontSpece);
		BattleCamera_BoundsBlendSpeed = json["BattleCamera_BoundsBlendSpeed"].GETFLOAT(BattleCamera_BoundsBlendSpeed);
		BattleCamera_WeightCurve = json["BattleCamera_WeightCurve"].GetAnimationCurve(BattleCamera_WeightCurve);
		BattleCamera.LOADFROMJSON(json["BattleCamera"]);
		CustomBattleCameraList.LoadFromJson(json["CustomBattleCameraList"]);
		Creature_DeadDisappearDelay = json["Creature_DeadDisappearDelay"].GETFLOAT(Creature_DeadDisappearDelay);
		CcKnockback_MaxDuration = json.GetJsonAny("CcKnockback_MaxDuration", "KnockbackMaxDuration").GETFLOAT(CcKnockback_MaxDuration);
		CcKnockback_PositionCurve = JsonTools.ToCurve(json.GetJsonAny("CcKnockback_PositionCurve", "KnockbackCurve"), CcKnockback_PositionCurve);
		CcDown_DownPositionDuration = json["CcDown_DownPositionDuration"].GETFLOAT(CcDown_DownPositionDuration);
		CcDown_DownPositionCurve = JsonTools.ToCurve(json["CcDown_DownPositionCurve"], CcDown_DownPositionCurve);
		CcDown_GetupDuration = json["CcDown_GetupDuration"].GETFLOAT(CcDown_GetupDuration);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("DamageFloatData", DamageFloatData);
		jsonData.AddFieldJsonMake("DamageDot", DamageDot);
		jsonData.AddFieldJsonMake("DamageFloatDataNew", DamageFloatDataNew);
		jsonData.AddFieldJsonMake("DamageDotNew", DamageDotNew);
		jsonData.AddFieldDouble("BattleCamera_WaveMoveFrontSpece", BattleCamera_WaveMoveFrontSpece);
		jsonData.AddFieldDouble("BattleCamera_BoundsBlendSpeed", BattleCamera_BoundsBlendSpeed);
		jsonData.AddFieldAnimationCurve("BattleCamera_WeightCurve", BattleCamera_WeightCurve);
		jsonData.AddFieldJsonMake("BattleCamera", BattleCamera);
		jsonData.AddFieldList("CustomBattleCameraList", CustomBattleCameraList);
		jsonData.AddFieldDouble("Creature_DeadDisappearDelay", Creature_DeadDisappearDelay);
		jsonData.AddFieldDouble("CcKnockback_MaxDuration", CcKnockback_MaxDuration);
		jsonData.AddField("CcKnockback_PositionCurve", JsonTools.FromCurve(CcKnockback_PositionCurve));
		jsonData.AddFieldDouble("CcDown_DownPositionDuration", CcDown_DownPositionDuration);
		jsonData.AddField("CcDown_DownPositionCurve", JsonTools.FromCurve(CcDown_DownPositionCurve));
		jsonData.AddFieldDouble("CcDown_GetupDuration", CcDown_GetupDuration);
		return jsonData;
	}
}
