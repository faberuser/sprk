using NGame2.NDevice;
using NGame2.NNotification;
using NGame2.NTutorial;
using NGameAsset.NSound;
using NVespa;
using UnityEngine;

namespace NGame2;

public class GameOptionData
{
	public enum ESoloTimeEffectType
	{
		High,
		Mid,
		Low
	}

	public enum EConsumeBattery
	{
		Low = 1,
		Middle = 2,
		High = 3,
		None = 0
	}

	public enum ESleepMode
	{
		None,
		Minute_10,
		Minute_30
	}

	public static bool EditorToggleAutoPlay = false;

	public static bool EditorToggleAutoRepeat = false;

	public static bool EditorToggleAutoSkill = false;

	public static bool EditorToggleAutoPvp = false;

	public static bool EditorToggleFastSpeed = false;

	public static LocalStorageString EditorAutoPvpPoint = new LocalStorageString(string.Empty, typeof(GameOptionData).FullName + "EditorAutoPvpPoint2");

	public static LocalStorageBool _noti_isDefaultValueSet = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_noti_isDefaultValueSet");

	private static LocalStorageBool _render_EnableSkillCamera = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_render_EnableSkillCamera");

	private static LocalStorageBool _render_EnableBossPatternAlarm = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_render_EnableBossPatternAlarm");

	private static LocalStorageEnum<ESoloTimeEffectType> _render_SoloTimeEffectOption = new LocalStorageEnum<ESoloTimeEffectType>(ESoloTimeEffectType.High, "KingsRaid_GameOption_render_SoloTimeEffectOption");

	private static LocalStorageBool _ = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_render_EnableSkillCamera");

	private static LocalStorageBool _device_EnableVibration = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_device_EnableVibration");

	private static LocalStorageBool _device_EnableNotification = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_device_EnableNotification");

	private static LocalStorageEnum<EConsumeBattery> _device_ConsumeBattery = new LocalStorageEnum<EConsumeBattery>(EConsumeBattery.None, "KingsRaid_GameOption_device_ConsumeBattery");

	private static LocalStorageEnum<ESleepMode> _device_SleepMode = new LocalStorageEnum<ESleepMode>(ESleepMode.None, "KingsRaid_GameOption_device_SleepMode");

	private static LocalStorageBool _EnableTranscend = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_EnableTranscend");

	private static LocalStorageBool _show_playGuide = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_show_playGuide");

	private static LocalStorageBool _show_questGuide = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_show_questGuide");

	public static bool EditorCanAutoPlay
	{
		get
		{
			if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
			{
				return false;
			}
			if (!EditorToggleAutoPlay)
			{
				return EditorToggleAutoRepeat;
			}
			return true;
		}
	}

	public static bool Render_EnableSkillCamera
	{
		get
		{
			return _render_EnableSkillCamera.Value;
		}
		set
		{
			_render_EnableSkillCamera.Value = value;
		}
	}

	public static bool Render_EnableBossPatternAlarm
	{
		get
		{
			return _render_EnableBossPatternAlarm.Value;
		}
		set
		{
			_render_EnableBossPatternAlarm.Value = value;
		}
	}

	public static ESoloTimeEffectType Render_SoloTimeEffectOption
	{
		get
		{
			return _render_SoloTimeEffectOption.Value;
		}
		set
		{
			_render_SoloTimeEffectOption.Value = value;
		}
	}

	public static bool Device_EnableVibration
	{
		get
		{
			return _device_EnableVibration.Value;
		}
		set
		{
			_device_EnableVibration.Value = value;
		}
	}

	public static bool Device_EnableNotification
	{
		get
		{
			return _device_EnableNotification.Value;
		}
		set
		{
			_device_EnableNotification.Value = value;
			ApplyNotification(isChangedRemote: true);
		}
	}

	public static EConsumeBattery Device_ConsumeBattery
	{
		get
		{
			return _device_ConsumeBattery.Value;
		}
		set
		{
			_device_ConsumeBattery.Value = value;
			ApplyConsumeBattery();
		}
	}

	public static ESleepMode Device_SleepMode
	{
		get
		{
			return _device_SleepMode.Value;
		}
		set
		{
			_device_SleepMode.Value = value;
			ApplySleepMode();
		}
	}

	public static bool Sound_EnableBgm
	{
		get
		{
			return SystemOptionData.Sound_EnableBgm;
		}
		set
		{
			SystemOptionData.Sound_EnableBgm = value;
		}
	}

	public static float Sound_BgmVolume
	{
		get
		{
			return SystemOptionData.Sound_BgmVolume;
		}
		set
		{
			SystemOptionData.Sound_BgmVolume = value;
		}
	}

	public static bool Sound_EnableGameSound
	{
		get
		{
			return SystemOptionData.Sound_EnableGameSound;
		}
		set
		{
			SystemOptionData.Sound_EnableGameSound = value;
		}
	}

	public static float Sound_GameSoundVolume
	{
		get
		{
			return SystemOptionData.Sound_GameSoundVolume;
		}
		set
		{
			SystemOptionData.Sound_GameSoundVolume = value;
		}
	}

	public static bool Sound_EnableCharacterVoice
	{
		get
		{
			return SystemOptionData.Sound_EnableCharacterVoice;
		}
		set
		{
			SystemOptionData.Sound_EnableCharacterVoice = value;
		}
	}

	public static SystemLanguage Sound_CharacterLanguage
	{
		get
		{
			return SingletonAuto<LanguageSetting>.instance.GetBaseLanguage();
		}
		set
		{
			SingletonAuto<LanguageSetting>.instance.SetBaseLanguage(value);
		}
	}

	public static float Sound_CharacterVoiceVolume
	{
		get
		{
			return SystemOptionData.Sound_CharacterVoiceVolume;
		}
		set
		{
			SystemOptionData.Sound_CharacterVoiceVolume = value;
		}
	}

	public static bool Sound_EnableAudioMixer
	{
		get
		{
			return SystemOptionData.Sound_EnableAudioMixer;
		}
		set
		{
			SystemOptionData.Sound_EnableAudioMixer = value;
		}
	}

	public static bool Noti_AllNoti
	{
		get
		{
			return SystemOptionData.Noti_AllNoti;
		}
		set
		{
			SystemOptionData.Noti_AllNoti = value;
			ApplyNotification(isChangedRemote: true);
		}
	}

	public static bool Noti_ArenaTicketNoti
	{
		get
		{
			return SystemOptionData.Noti_ArenaTicketNoti;
		}
		set
		{
			SystemOptionData.Noti_ArenaTicketNoti = value;
			ApplyNotification();
		}
	}

	public static bool Noti_ChallengeTowerNoti
	{
		get
		{
			return SystemOptionData.Noti_ChallengeTowerNoti;
		}
		set
		{
			SystemOptionData.Noti_ChallengeTowerNoti = value;
			ApplyNotification();
		}
	}

	public static bool Noti_EventNoti
	{
		get
		{
			return SystemOptionData.Noti_EventNoti;
		}
		set
		{
			SystemOptionData.Noti_EventNoti = value;
			ApplyNotification();
		}
	}

	public static bool Noti_GuildRaidOpenNoti
	{
		get
		{
			return SystemOptionData.Noti_GuildRaidOpenNoti;
		}
		set
		{
			SystemOptionData.Noti_GuildRaidOpenNoti = value;
			ApplyNotification();
		}
	}

	public static bool Noti_ItemCompleteNoti
	{
		get
		{
			return SystemOptionData.Noti_ItemCompleteNoti;
		}
		set
		{
			SystemOptionData.Noti_ItemCompleteNoti = value;
			ApplyNotification();
		}
	}

	public static bool Noti_NightNoti
	{
		get
		{
			return SystemOptionData.Noti_NightNoti;
		}
		set
		{
			SystemOptionData.Noti_NightNoti = value;
			ApplyNotification();
		}
	}

	public static bool Noti_StaminaFullNoti
	{
		get
		{
			return SystemOptionData.Noti_StaminaFullNoti;
		}
		set
		{
			SystemOptionData.Noti_StaminaFullNoti = value;
			ApplyNotification();
		}
	}

	public static bool Noti_SystemNoti
	{
		get
		{
			return SystemOptionData.Noti_SystemNotifi;
		}
		set
		{
			SystemOptionData.Noti_SystemNotifi = value;
			ApplyNotification();
		}
	}

	public static bool Noti_GuildSuppressOpenNoti
	{
		get
		{
			return SystemOptionData.Noti_GuildSupressOpenNoti;
		}
		set
		{
			SystemOptionData.Noti_GuildSupressOpenNoti = value;
			ApplyNotification();
		}
	}

	public static bool Show_PlayGuide
	{
		get
		{
			return _show_playGuide.Value;
		}
		set
		{
			_show_playGuide.Value = value;
		}
	}

	public static bool Show_QuestGuide
	{
		get
		{
			return _show_questGuide.Value;
		}
		set
		{
			_show_questGuide.Value = value;
		}
	}

	public static void Init()
	{
		ApplyNotification(isChangedRemote: true);
	}

	private static void ApplyNotification(bool isChangedRemote = false)
	{
		SingletonAuto<NotificationManager>.instance.SetEnabled(Noti_AllNoti, Noti_SystemNoti, Noti_StaminaFullNoti, Noti_ChallengeTowerNoti, Noti_ArenaTicketNoti, Noti_ItemCompleteNoti, Noti_GuildRaidOpenNoti, Noti_EventNoti, Noti_NightNoti, isChangedRemote);
	}

	private static void ApplyConsumeBattery()
	{
		if (Device_ConsumeBattery == EConsumeBattery.Low)
		{
			Device_ConsumeBattery = EConsumeBattery.Middle;
		}
		if (Device_ConsumeBattery == EConsumeBattery.Low)
		{
			Application.targetFrameRate = 30;
		}
		else if (Device_ConsumeBattery == EConsumeBattery.Middle)
		{
			Application.targetFrameRate = 30;
		}
		else
		{
			Application.targetFrameRate = 60;
		}
	}

	public static void ApplySleepMode()
	{
		if (Device_SleepMode == ESleepMode.Minute_10)
		{
			SingletonAuto<SleepModeManager>.instance.SetSleepTimeout(600);
		}
		else if (Device_SleepMode == ESleepMode.Minute_30)
		{
			SingletonAuto<SleepModeManager>.instance.SetSleepTimeout(1800);
		}
		else
		{
			SingletonAuto<SleepModeManager>.instance.DisableSleepTimeout();
		}
	}
}
