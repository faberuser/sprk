using JsonBin;
using NVespa;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class EffectFxData : ILoadJson
{
	public enum ETriggerType
	{
		Area,
		Event
	}

	public enum EDisplayType
	{
		Normal,
		Loop,
		Screen,
		AttachToModel
	}

	public enum EDisplayType2
	{
		Normal,
		Screen,
		AttachToModel
	}

	public enum ECheckResult
	{
		Enable,
		Disable,
		Skip
	}

	public static ECheckResult ForceAreaCheckResult = ECheckResult.Skip;

	public string Description = string.Empty;

	public string Code = string.Empty;

	public ETriggerType TriggerType;

	public Rect EnableArea = Rect.MinMaxRect(-10000f, -10000f, 10000f, 10000f);

	public bool IsLookatTrigger;

	public EBattleFieldEventType BattleFieldEventPlay = EBattleFieldEventType.WaveStart;

	public EBattleFieldEventType BattleFieldEventStop;

	public int EventPlayValue;

	public int EventStopValue;

	public EDisplayType2 DisplayType2;

	public bool IsBillboard;

	public bool IsLoop;

	public float LoopLength = 24f;

	public string AttachPath = string.Empty;

	public bool PlayOnce;

	public TransformData FxTransform = new TransformData();

	public ECheckResult CheckEnableArea(Camera cam, GameObject mapGo)
	{
		if (ForceAreaCheckResult == ECheckResult.Enable)
		{
			return ECheckResult.Enable;
		}
		if (ForceAreaCheckResult == ECheckResult.Disable)
		{
			return ECheckResult.Disable;
		}
		Vector3 vector = mapGo.transform.worldToLocalMatrix.MultiplyPoint(cam.transform.position);
		Vector3 point;
		if (!IsLookatTrigger)
		{
			point = vector;
		}
		else
		{
			Vector3 vector2 = mapGo.transform.worldToLocalMatrix.MultiplyVector(cam.transform.forward);
			if (vector2.y > -0.01f)
			{
				return ECheckResult.Skip;
			}
			if (vector.y < 0.01f)
			{
				return ECheckResult.Skip;
			}
			float num = 0f - vector.y / vector2.y;
			point = vector + vector2 * num;
		}
		if (EnableArea.Contains(point))
		{
			return ECheckResult.Enable;
		}
		if (!Vespa_Unity_Rect_Extension_Method.EnlargeRect(EnableArea, 0.5f, 0.5f).Contains(point))
		{
			return ECheckResult.Disable;
		}
		return ECheckResult.Skip;
	}

	public bool IsPlayEvent(EBattleFieldEventType eventType, int eventValue)
	{
		if (eventType != BattleFieldEventPlay)
		{
			return false;
		}
		if (eventType == EBattleFieldEventType.WaveReady || eventType == EBattleFieldEventType.WaveStart || eventType == EBattleFieldEventType.WaveEnd)
		{
			if (EventPlayValue == 0)
			{
				return true;
			}
			return EventPlayValue == eventValue;
		}
		return false;
	}

	public bool IsStopEvent(EBattleFieldEventType eventType, int eventValue)
	{
		if (eventType != BattleFieldEventStop)
		{
			return false;
		}
		if (eventType == EBattleFieldEventType.WaveReady || eventType == EBattleFieldEventType.WaveStart || eventType == EBattleFieldEventType.WaveEnd)
		{
			if (EventPlayValue == 0)
			{
				return true;
			}
			return EventStopValue == eventValue;
		}
		return false;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Description = json["Description"].GETSTRING(Description);
		Code = json["Code"].GETSTRING(Code);
		TriggerType = json["TriggerType"].GETENUM(TriggerType);
		EnableArea = json["EnableArea"].GETRECT(EnableArea);
		IsLookatTrigger = json["IsLookatTrigger"].GETBOOLEAN(IsLookatTrigger);
		BattleFieldEventPlay = json["BattleFieldEventPlay"].GETENUM(BattleFieldEventPlay);
		BattleFieldEventStop = json["BattleFieldEventStop"].GETENUM(BattleFieldEventStop);
		EventPlayValue = json["EventPlayValue"].GETINT(EventPlayValue);
		EventStopValue = json["EventStopValue"].GETINT(EventStopValue);
		DisplayType2 = json["DisplayType2"].GETENUM(DisplayType2);
		IsBillboard = json["IsBillboard"].GETBOOLEAN(IsBillboard);
		IsLoop = json["IsLoop"].GETBOOLEAN(IsLoop);
		LoopLength = json["LoopLength"].GETFLOAT(LoopLength);
		AttachPath = json.GetJsonAny("ModelRendererPath", "AttachPath").GETSTRING(AttachPath);
		PlayOnce = json["PlayOnce"].GETBOOLEAN(PlayOnce);
		FxTransform.LOADFROMJSON(json["FxTransform"]);
		if (json.Contains("DisplayType"))
		{
			switch (json["DisplayType"].GETENUM(EDisplayType.Normal))
			{
			case EDisplayType.Normal:
				DisplayType2 = EDisplayType2.Normal;
				break;
			case EDisplayType.Screen:
				DisplayType2 = EDisplayType2.Screen;
				break;
			case EDisplayType.AttachToModel:
				DisplayType2 = EDisplayType2.AttachToModel;
				break;
			case EDisplayType.Loop:
				DisplayType2 = EDisplayType2.Normal;
				IsLoop = true;
				break;
			}
		}
		return true;
	}
}
