using System.Collections.Generic;
using LitJson;
using NGame2;
using NGame2.NBattleContext;
using NShared;
using UnityEngine;

namespace NVespa.NGameSharedSetting;

public class ValueData : IBuilder, ILoadJson, IJsonInterface, IMakeJson, IBattleContextChangeListner
{
	public class TimerEventData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public string Name = "";

		public bool DisableSkillCamera;

		public float ExtraAlpha;

		public float ExtraAlphaBattleField;

		public float ColorEffect = 0.9f;

		public float FieldColorEffect = 0.9f;

		public Color SoloColor = new Color(0f, 0f, 0f, 1f);

		public float CreatureDamageColorMultiply = 1f;

		bool ILoadJson.Load(JsonData json)
		{
			Name = json["Name"].GETSTRING(Name);
			DisableSkillCamera = json["DisableSkillCamera"].GETBOOLEAN(DisableSkillCamera);
			ExtraAlpha = json["ExtraAlpha"].GETFLOAT(ExtraAlpha);
			ExtraAlphaBattleField = json["ExtraAlphaBattleField"].GETFLOAT(ExtraAlphaBattleField);
			ColorEffect = json["ColorEffect"].GETFLOAT(ColorEffect);
			FieldColorEffect = json["FieldColorEffect"].GETFLOAT(FieldColorEffect);
			SoloColor = json["SoloColor"].GETCOLOR(SoloColor);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("Name", Name);
			jsonData.AddFieldBool("DisableSkillCamera", DisableSkillCamera);
			jsonData.AddFieldDouble("ExtraAlpha", ExtraAlpha);
			jsonData.AddFieldDouble("ExtraAlphaBattleField", ExtraAlphaBattleField);
			jsonData.AddFieldDouble("ColorEffect", ColorEffect);
			jsonData.AddFieldDouble("FieldColorEffect", FieldColorEffect);
			jsonData.AddFieldColor("SoloColor", SoloColor);
			return jsonData;
		}
	}

	private TimerEventData _defaultTimeEventData = new TimerEventData();

	public AnimationCurve ShakeMaker_PowerByLifeTime = new AnimationCurve();

	public List<TimerEventData> TimerEvent = new List<TimerEventData>();

	public List<TimerEventData> CustomTimerEventList = new List<TimerEventData>();

	private Dictionary<string, TimerEventData> _timerEventDic;

	private Dictionary<string, TimerEventData> _customTimerEventDic;

	private TimerEventData _curData;

	public void OnBattleContextChange()
	{
		_curData = null;
	}

	public TimerEventData GetTimerEvent()
	{
		if (_curData != null)
		{
			return _curData;
		}
		string text = string.Empty;
		if (SingletonAuto<BattleContext>.isValidInstance && SingletonAuto<BattleContext>.instance.DataProvider != null)
		{
			BattleDefineData battleDefine = SingletonAuto<BattleContext>.instance.DataProvider.GetBattleDefine();
			if (battleDefine != null)
			{
				text = battleDefine.BattleCameraName;
			}
		}
		NGame2.GameOptionData.ESoloTimeEffectType render_SoloTimeEffectOption = NGame2.GameOptionData.Render_SoloTimeEffectOption;
		string i_key = render_SoloTimeEffectOption.ToString();
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_timerEventDic, i_key, _defaultTimeEventData);
		}
		if (render_SoloTimeEffectOption == NGame2.GameOptionData.ESoloTimeEffectType.High)
		{
			_curData = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_customTimerEventDic, text, _defaultTimeEventData);
		}
		else
		{
			TimerEventData vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_customTimerEventDic, text, _defaultTimeEventData);
			_curData = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_timerEventDic, i_key, _defaultTimeEventData);
			_curData.DisableSkillCamera = vALUE.DisableSkillCamera;
		}
		return _curData;
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		ShakeMaker_PowerByLifeTime = json["ShakeMaker_PowerByLifeTime"].GetAnimationCurve(ShakeMaker_PowerByLifeTime);
		TimerEvent.LoadFromJson(json["TimerEventList"]);
		_timerEventDic = new Dictionary<string, TimerEventData>();
		for (int i = 0; i < TimerEvent.Count; i++)
		{
			TimerEventData timerEventData = TimerEvent[i];
			_timerEventDic.Add(timerEventData.Name, timerEventData);
		}
		CustomTimerEventList.LoadFromJson(json["CustomTimerEventList"]);
		_customTimerEventDic = new Dictionary<string, TimerEventData>();
		for (int j = 0; j < CustomTimerEventList.Count; j++)
		{
			TimerEventData timerEventData2 = CustomTimerEventList[j];
			_customTimerEventDic.Add(timerEventData2.Name, timerEventData2);
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldAnimationCurve("ShakeMaker_PowerByLifeTime", ShakeMaker_PowerByLifeTime);
		jsonData.AddFieldList("TimerEventList", TimerEvent);
		jsonData.AddFieldList("CustomTimerEventList", CustomTimerEventList);
		return jsonData;
	}
}
