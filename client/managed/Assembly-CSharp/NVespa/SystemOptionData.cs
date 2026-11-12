using NGame2;
using NGame2.NBattleView.NUIOn3d2;
using NGame2.NPatch;
using NGame2.NUtil;
using NGameAsset;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa;

public static class SystemOptionData
{
	private static LocalStorageBool _graphicFrameAutoControl;

	private static LocalStorageEnum<EFrameControl> _graphicFrameContorl;

	private static LocalStorageEnum<EResolutionControl> _graphicResolutionContorl;

	private static LocalStorageBool _graphicWindowContorl;

	private static LocalStorageBool _graphicVsyncContorl;

	private static LocalStorageInt _graphicTargetFrameRate;

	private static LocalStorageEnum<ESpecType> _spec;

	private static LocalStorageEnum<EVideoSpecType> _video_spec;

	private static LocalStorageEnum<EDownloadType> _downloadType;

	private static LocalStorageBool _tryBackgroundDownload;

	private static LocalStorageBool _showTitleGameIntroduce;

	private static LocalStorageBool _showBuff;

	private static LocalStorageBool _showBuffAlly;

	private static LocalStorageBool _showBuffEnemy;

	private static LocalStorageBool _render_EnableCartoonShader;

	private static LocalStorageEnum<EShadowType> _render_ShadowType;

	private static LocalStorageBool _render_EnableRimLight;

	private static LocalStorageBool _render_EnableOutline;

	private static LocalStorageEnum<ERenderDamageText> _render_DamageText;

	private static LocalStorageBool _showDamageUnit;

	private static LocalStorageBool _showFontNew;

	private static LocalStorageBool _render_ShowSkillEffect;

	private static LocalStorageBool _render_ShowAllySkillEffect;

	private static LocalStorageBool _render_ShowEnemySkillEffect;

	private static LocalStorageBool _sound_EnableBgm;

	private static LocalStorageFloat _sound_BgmVolume;

	private static LocalStorageBool _sound_EnableGameSound;

	private static LocalStorageFloat _sound_GameSoundVolume;

	private static LocalStorageBool _sound_EnableCharacterVoice;

	private static LocalStorageFloat _sound_CharacterVoiceVolume;

	private static LocalStorageBool _sound_EnableAudioMixer;

	private static LocalStorageBool _chat_AllowAllWhisper;

	private static LocalStorageBool _chat_AllowFriendWhisper;

	private static LocalStorageBool _chat_AllowGuildWhisper;

	private static LocalStorageBool _chat_AllowEtcWhisper;

	private static LocalStorageBool _chat_ShowBattleEmoticon;

	private static LocalStorageBool _costume_AllowCutSceneAccessory;

	private static LocalAccountInt _rubyPopup_Skip_Count;

	private static LocalStorageBool _IsWindow;

	private static LocalStorageBool _noti_AllNoti;

	private static LocalStorageBool _noti_SystemNotifi;

	private static LocalStorageBool _noti_StaminaFullNoti;

	private static LocalStorageBool _noti_ChallengeTowerNoti;

	private static LocalStorageBool _noti_ArenaTicketNoti;

	private static LocalStorageBool _noti_ItemCompleteNoti;

	private static LocalStorageBool _noti_GuildRaidOpenNoti;

	private static LocalStorageBool _noti_EventNoti;

	private static LocalStorageBool _noti_NightNoti;

	private static LocalStorageBool _checkOnceAfterInstall;

	private static LocalStorageBool _noti_GuildSuppressOpenNoti;

	private static bool _sprkSettingsRestored;

	public static ESpecType Spec
	{
		get
		{
			return _spec.Value;
		}
		set
		{
			_spec.Value = value;
			SprkTestPrefs.Save();
		}
	}

	public static EVideoSpecType VideoSpec
	{
		get
		{
			return _video_spec.Value;
		}
		set
		{
			_video_spec.Value = value;
		}
	}

	public static EDownloadType DownloadType
	{
		get
		{
			return _downloadType.Value;
		}
		set
		{
			_downloadType.Value = value;
		}
	}

	public static bool TryBackgroundDownload
	{
		get
		{
			return _tryBackgroundDownload.Value;
		}
		set
		{
			_tryBackgroundDownload.Value = value;
		}
	}

	public static bool ShowTitleGameIntroduce
	{
		get
		{
			return _showTitleGameIntroduce.Value;
		}
		set
		{
			_showTitleGameIntroduce.Value = value;
		}
	}

	public static bool Graphic_IsAutoFrameEnabled
	{
		get
		{
			return _graphicFrameAutoControl.Value;
		}
		set
		{
			_graphicFrameAutoControl.Value = value;
		}
	}

	public static EFrameControl Graphic_FrameControl
	{
		get
		{
			return _graphicFrameContorl.Value;
		}
		set
		{
			_graphicFrameContorl.Value = value;
			ApplyFrameRateSetting();
		}
	}

	public static EResolutionControl Graphic_ResolutionControl
	{
		get
		{
			return _graphicResolutionContorl.Value;
		}
		set
		{
			_graphicResolutionContorl.Value = value;
			ApplyResoultionSetting();
		}
	}

	public static bool Graphic_WindowControl
	{
		get
		{
			return _graphicWindowContorl.Value;
		}
		set
		{
			_graphicWindowContorl.Value = value;
			ApplyWindowSetting();
		}
	}

	public static bool Graphic_VSyncControl
	{
		get
		{
			return _graphicVsyncContorl.Value;
		}
		set
		{
			_graphicVsyncContorl.Value = value;
			ApplySyncSetting();
		}
	}

	public static int Graphic_TargetFrameRate
	{
		get
		{
			return _graphicTargetFrameRate.Value;
		}
		set
		{
			_graphicTargetFrameRate.Value = value;
			SetTargetFrameRate();
		}
	}

	public static bool ShowBuff
	{
		get
		{
			return _showBuff.Value;
		}
		set
		{
			_showBuff.Value = value;
		}
	}

	public static bool ShowBuffAlly
	{
		get
		{
			return _showBuffAlly.Value;
		}
		set
		{
			_showBuffAlly.Value = value;
		}
	}

	public static bool ShowBuffEnemy
	{
		get
		{
			return _showBuffEnemy.Value;
		}
		set
		{
			_showBuffEnemy.Value = value;
		}
	}

	public static bool Render_EnableCartoonShader
	{
		get
		{
			return _render_EnableCartoonShader.Value;
		}
		set
		{
			_render_EnableCartoonShader.Value = value;
			ApplyShaderLod();
		}
	}

	public static EShadowType Render_ShadowType
	{
		get
		{
			return _render_ShadowType.Value;
		}
		set
		{
			_render_ShadowType.Value = value;
			ApplyShaderLod();
		}
	}

	public static bool Render_EnableRimLight
	{
		get
		{
			return _render_EnableRimLight.Value;
		}
		set
		{
			_render_EnableRimLight.Value = value;
			ApplyShaderLod();
		}
	}

	public static bool Render_EnableOutline
	{
		get
		{
			return _render_EnableOutline.Value;
		}
		set
		{
			_render_EnableOutline.Value = value;
			ApplyShaderLod();
		}
	}

	public static ERenderDamageText Render_DamageText
	{
		get
		{
			return _render_DamageText.Value;
		}
		set
		{
			_render_DamageText.Value = value;
			ApplyDamageRender();
		}
	}

	public static bool ShowDamageUnit
	{
		get
		{
			return _showDamageUnit.Value;
		}
		set
		{
			_showDamageUnit.Value = value;
		}
	}

	public static bool ShowFontNew
	{
		get
		{
			return _showFontNew.Value;
		}
		set
		{
			_showFontNew.Value = value;
		}
	}

	public static bool Render_ShowSkillEffect
	{
		get
		{
			return _render_ShowSkillEffect.Value;
		}
		set
		{
			_render_ShowSkillEffect.Value = value;
		}
	}

	public static bool Render_ShowAllySkillEffect
	{
		get
		{
			return _render_ShowAllySkillEffect.Value;
		}
		set
		{
			_render_ShowAllySkillEffect.Value = value;
		}
	}

	public static bool Render_ShowEnemySkillEffect
	{
		get
		{
			return _render_ShowEnemySkillEffect.Value;
		}
		set
		{
			_render_ShowEnemySkillEffect.Value = value;
		}
	}

	public static bool Sound_EnableBgm
	{
		get
		{
			return _sound_EnableBgm.Value;
		}
		set
		{
			_sound_EnableBgm.Value = value;
		}
	}

	public static float Sound_BgmVolume
	{
		get
		{
			return _sound_BgmVolume.Value;
		}
		set
		{
			_sound_BgmVolume.Value = value;
		}
	}

	public static bool Sound_EnableGameSound
	{
		get
		{
			return _sound_EnableGameSound.Value;
		}
		set
		{
			_sound_EnableGameSound.Value = value;
			UpdateAssetSound();
		}
	}

	public static float Sound_GameSoundVolume
	{
		get
		{
			return _sound_GameSoundVolume.Value;
		}
		set
		{
			_sound_GameSoundVolume.Value = value;
		}
	}

	public static bool Sound_EnableCharacterVoice
	{
		get
		{
			return _sound_EnableCharacterVoice.Value;
		}
		set
		{
			_sound_EnableCharacterVoice.Value = value;
			UpdateAssetSound();
		}
	}

	public static float Sound_CharacterVoiceVolume
	{
		get
		{
			return _sound_CharacterVoiceVolume.Value;
		}
		set
		{
			_sound_CharacterVoiceVolume.Value = value;
		}
	}

	public static bool Sound_EnableAudioMixer
	{
		get
		{
			return _sound_EnableAudioMixer.Value;
		}
		set
		{
			_sound_EnableAudioMixer.Value = value;
			UpdateAssetSound();
		}
	}

	public static bool Chat_AllowAllWhisper
	{
		get
		{
			return _chat_AllowAllWhisper.Value;
		}
		set
		{
			_chat_AllowAllWhisper.Value = value;
		}
	}

	public static bool Chat_AllowFriendWhisper
	{
		get
		{
			return _chat_AllowFriendWhisper.Value;
		}
		set
		{
			_chat_AllowFriendWhisper.Value = value;
		}
	}

	public static bool Chat_AllowGuildWhisper
	{
		get
		{
			return _chat_AllowGuildWhisper.Value;
		}
		set
		{
			_chat_AllowGuildWhisper.Value = value;
		}
	}

	public static bool Chat_AllowEtcWhisper
	{
		get
		{
			return _chat_AllowEtcWhisper.Value;
		}
		set
		{
			_chat_AllowEtcWhisper.Value = value;
		}
	}

	public static bool Show_BattleEmoticon
	{
		get
		{
			return _chat_ShowBattleEmoticon.Value;
		}
		set
		{
			_chat_ShowBattleEmoticon.Value = value;
		}
	}

	public static bool Costume_AllowCutSceneAccessory
	{
		get
		{
			return _costume_AllowCutSceneAccessory.Value;
		}
		set
		{
			_costume_AllowCutSceneAccessory.Value = value;
		}
	}

	public static int RubyPopup_Skip_Count
	{
		get
		{
			return _rubyPopup_Skip_Count.Value;
		}
		set
		{
			_rubyPopup_Skip_Count.Value = value;
		}
	}

	public static bool Noti_AllNoti
	{
		get
		{
			return _noti_AllNoti.Value;
		}
		set
		{
			_noti_AllNoti.Value = value;
		}
	}

	public static bool Noti_SystemNotifi
	{
		get
		{
			return _noti_SystemNotifi.Value;
		}
		set
		{
			_noti_SystemNotifi.Value = value;
		}
	}

	public static bool Noti_StaminaFullNoti
	{
		get
		{
			return _noti_StaminaFullNoti.Value;
		}
		set
		{
			_noti_StaminaFullNoti.Value = value;
		}
	}

	public static bool Noti_ChallengeTowerNoti
	{
		get
		{
			return _noti_ChallengeTowerNoti.Value;
		}
		set
		{
			_noti_ChallengeTowerNoti.Value = value;
		}
	}

	public static bool Noti_ArenaTicketNoti
	{
		get
		{
			return _noti_ArenaTicketNoti.Value;
		}
		set
		{
			_noti_ArenaTicketNoti.Value = value;
		}
	}

	public static bool Noti_ItemCompleteNoti
	{
		get
		{
			return _noti_ItemCompleteNoti.Value;
		}
		set
		{
			_noti_ItemCompleteNoti.Value = value;
		}
	}

	public static bool Noti_GuildRaidOpenNoti
	{
		get
		{
			return _noti_GuildRaidOpenNoti.Value;
		}
		set
		{
			_noti_GuildRaidOpenNoti.Value = value;
		}
	}

	public static bool Noti_EventNoti
	{
		get
		{
			return _noti_EventNoti.Value;
		}
		set
		{
			_noti_EventNoti.Value = value;
		}
	}

	public static bool Noti_NightNoti
	{
		get
		{
			return _noti_NightNoti.Value;
		}
		set
		{
			_noti_NightNoti.Value = value;
		}
	}

	public static bool Noti_GuildSupressOpenNoti
	{
		get
		{
			if ((!_noti_AllNoti.Value || !_noti_SystemNotifi.Value) && _noti_GuildSuppressOpenNoti.Value)
			{
				_noti_GuildSuppressOpenNoti.Value = false;
			}
			return _noti_GuildSuppressOpenNoti.Value;
		}
		set
		{
			_noti_GuildSuppressOpenNoti.Value = value;
		}
	}

	static SystemOptionData()
	{
		_graphicFrameAutoControl = new LocalStorageBool(i_default: false, "KingsRaid_SystemOption_graphicFrameAutoControl");
		_graphicFrameContorl = new LocalStorageEnum<EFrameControl>(EFrameControl.Auto, "KingsRaid_SystemOption_graphicFrameContorl");
		_graphicResolutionContorl = new LocalStorageEnum<EResolutionControl>(EResolutionControl.FHD, "KingsRaid_SystemOption_graphicResolutionContorl");
		_graphicWindowContorl = new LocalStorageBool(i_default: true, "KingsRaid_SystemOption_graphicWindowContorl");
		_graphicVsyncContorl = new LocalStorageBool(i_default: true, "KingsRaid_SystemOption_VsyncContorl");
		_graphicTargetFrameRate = new LocalStorageInt(60, "KingsRaid_SystemOption_graphicTargetFrameRate");
		_spec = new LocalStorageEnum<ESpecType>(ESpecType.Undefine, "KingsRaid_SystemOption_spec");
		_video_spec = new LocalStorageEnum<EVideoSpecType>(EVideoSpecType.Undefine, "KingsRaid_SystemOption_video_spec");
		_downloadType = new LocalStorageEnum<EDownloadType>(EDownloadType.Undefine, "KingsRaid_SystemOption_downloadType");
		_tryBackgroundDownload = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_tryBackgroundDownload");
		_showTitleGameIntroduce = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_showTitleGameIntroduce");
		_showBuff = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_showBuff");
		_showBuffAlly = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_showBuffAlly");
		_showBuffEnemy = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_showBuffEnemy");
		_render_EnableCartoonShader = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_render_EnableCartoonShader");
		_render_ShadowType = new LocalStorageEnum<EShadowType>(EShadowType.Normal, "KingsRaid_GameOption_render_ShadowType");
		_render_EnableRimLight = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_render_EnableRimLight");
		_render_EnableOutline = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_render_EnableOutline3");
		_render_DamageText = new LocalStorageEnum<ERenderDamageText>(ERenderDamageText.ShowAll, "KingsRaid_GameOption_render_DamageText");
		_showDamageUnit = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_showDamageUnit");
		_showFontNew = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_showFontNew");
		_render_ShowSkillEffect = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_render_ShowSkillEffect");
		_render_ShowAllySkillEffect = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_render_ShowAllySkillEffect");
		_render_ShowEnemySkillEffect = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_render_ShowEnemySkillEffect");
		_sound_EnableBgm = new LocalStorageBool(i_default: true, "KingsRaid_SystemOption_sound_EnableBgm");
		_sound_BgmVolume = new LocalStorageFloat(1f, "KingsRaid_SystemOption_sound_BgmVolume");
		_sound_EnableGameSound = new LocalStorageBool(i_default: true, "KingsRaid_SystemOption_sound_EnableGameSound");
		_sound_GameSoundVolume = new LocalStorageFloat(1f, "KingsRaid_SystemOption_sound_GameSoundVolume");
		_sound_EnableAudioMixer = new LocalStorageBool(i_default: true, "KingsRaid_SystemOption_sound_EnableAudioMixer");
		_chat_AllowAllWhisper = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_chat_AllowAllWhisper");
		_chat_AllowFriendWhisper = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_chat_AllowFriendWhisper");
		_chat_AllowGuildWhisper = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_chat_AllowGuildWhisper");
		_chat_AllowEtcWhisper = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_chat_AllowEtcWhisper");
		_chat_ShowBattleEmoticon = new LocalStorageBool(i_default: true, "KingRaid_GameOption_chat_ShowBattleEmoticon");
		_costume_AllowCutSceneAccessory = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_costume_AllowCutSceneAccessory");
		_rubyPopup_Skip_Count = new LocalAccountInt(0, "KingsRaid_GameOption_rubyPopup_Skip_Count");
		_IsWindow = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_IsWindow");
		_noti_AllNoti = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_noti_AllNoti");
		_noti_SystemNotifi = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_noti_SystemNoti");
		_noti_StaminaFullNoti = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_noti_StaminaFullNoti");
		_noti_ChallengeTowerNoti = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_noti_ChallengeTowerNoti");
		_noti_ArenaTicketNoti = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_noti_ArenaTicketNoti");
		_noti_ItemCompleteNoti = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_noti_ItemCompleteNoti");
		_noti_GuildRaidOpenNoti = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_noti_GuildRaidOpenNoti");
		_noti_EventNoti = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_noti_EventNoti");
		_noti_NightNoti = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_noti_NightNoti");
		_checkOnceAfterInstall = new LocalStorageBool(i_default: false, "KingsRaid_GameOption_checkLowMemoryDeviceOnce");
		_noti_GuildSuppressOpenNoti = new LocalStorageBool(i_default: true, "KingsRaid_GameOption_noti_GuildSuppressOpenNoti");
		_sound_EnableCharacterVoice = new LocalStorageBool(_sound_EnableGameSound.Value, "KingsRaid_SystemOption_sound_EnableCharacterVoice");
		_sound_CharacterVoiceVolume = new LocalStorageFloat(_sound_GameSoundVolume.Value, "KingsRaid_SystemOption_sound_CharacterVoiceVolume");
	}

	public static void Init()
	{
		ApplyShaderLod();
		ApplyDamageRender();
		CheckExistFrameRateOption();
		ApplyFrameRateSetting();
		LoadPatchDownloadSpecTableData();
		SetOnceGraphicOptionForLowMemoryUser();
	}

	private static void UpdateAssetSound()
	{
		if (!Sound_EnableGameSound && !Sound_EnableCharacterVoice)
		{
			SingletonAuto<SoundContainer>.instance.StopAll();
		}
	}

	private static void CheckExistFrameRateOption()
	{
		if (!SprkTestPrefs.HasKey("KingsRaid_SystemOption_graphicFrameContorl"))
		{
			switch (NGame2.GameOptionData.Device_ConsumeBattery)
			{
			case NGame2.GameOptionData.EConsumeBattery.High:
				_graphicFrameContorl.Value = EFrameControl.Manual_60;
				break;
			case NGame2.GameOptionData.EConsumeBattery.Low:
			case NGame2.GameOptionData.EConsumeBattery.Middle:
				_graphicFrameContorl.Value = EFrameControl.Manual_30;
				break;
			}
			NGame2.GameOptionData.Device_ConsumeBattery = NGame2.GameOptionData.EConsumeBattery.None;
		}
	}

	private static void ApplyResoultionSetting()
	{
		if (!SprkGraphicsOptions.ApplyCustomResolution())
		{
			switch (Graphic_ResolutionControl)
			{
			case EResolutionControl.HD:
				Screen.SetResolution(1280, 720, _IsWindow.Value);
				break;
			case EResolutionControl.FHD:
				Screen.SetResolution(1920, 1080, _IsWindow.Value);
				break;
			case EResolutionControl.QHD:
				Screen.SetResolution(2560, 1440, _IsWindow.Value);
				break;
			case EResolutionControl.UHD:
				Screen.SetResolution(3840, 2160, _IsWindow.Value);
				break;
			}
		}
	}

	private static void ApplyWindowSetting()
	{
		if (Graphic_WindowControl)
		{
			_IsWindow.Value = false;
			Screen.fullScreenMode = FullScreenMode.Windowed;
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.Windowed);
			ApplyResoultionSetting();
		}
		else
		{
			_IsWindow.Value = true;
			Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
		}
	}

	private static void ApplySyncSetting()
	{
		if (Graphic_VSyncControl)
		{
			QualitySettings.vSyncCount = 1;
			Debug.Log("VSync: ON (Every V Blank)");
		}
		else
		{
			QualitySettings.vSyncCount = 0;
			Debug.Log("VSync: OFF");
		}
	}

	private static void ApplyFrameRateSetting()
	{
		EFrameControl graphic_FrameControl = Graphic_FrameControl;
		if (graphic_FrameControl == EFrameControl.Auto || graphic_FrameControl == EFrameControl.AutoPlus)
		{
			Graphic_IsAutoFrameEnabled = true;
			GlobalAutoBehaviour<FrameManager>.instance.FrameStackReset();
		}
		else
		{
			Graphic_IsAutoFrameEnabled = false;
			SetTargetFrameRate();
		}
	}

	private static void SetTargetFrameRate()
	{
		Application.targetFrameRate = (int)Graphic_FrameControl;
		QualitySettings.vSyncCount = 0;
	}

	private static void ApplyDamageRender()
	{
		if (Render_DamageText == ERenderDamageText.ShowAll)
		{
			NGame2.NBattleView.NUIOn3d2.Damage.SetVisibleDamage(visible: true);
		}
		else
		{
			NGame2.NBattleView.NUIOn3d2.Damage.SetVisibleDamage(visible: false);
		}
	}

	private static void ApplyShaderLod()
	{
		int num;
		if (Render_EnableCartoonShader)
		{
			num = 3000;
			if (Render_ShadowType == EShadowType.Normal)
			{
				num |= 1;
			}
			if (Render_EnableRimLight)
			{
				num |= 2;
			}
			if (Render_EnableOutline)
			{
				num |= 4;
			}
		}
		else
		{
			num = 2999;
		}
		Shader.globalMaximumLOD = num;
	}

	public static void SetOnceGraphicOptionForLowMemoryUser()
	{
		bool num = _spec.Value != ESpecType.Undefine;
		bool flag = IsLowMemoryUser();
		if (!num && flag)
		{
			_checkOnceAfterInstall.Value = true;
			SetGraphicOptionLow();
		}
	}

	private static bool IsLowMemoryUser()
	{
		PatchDownloadSpecData recommandSpecData = PatchDownloadHelper.GetRecommandSpecData();
		if (recommandSpecData.Spec == PatchSpecType.Low && recommandSpecData.Platform != PatchPlatform.x86)
		{
			return true;
		}
		return false;
	}

	private static void LoadPatchDownloadSpecTableData()
	{
	}

	private static void SetGraphicOptionLow()
	{
		Render_EnableCartoonShader = false;
		Render_ShadowType = EShadowType.Off;
		Render_EnableRimLight = false;
		Render_EnableOutline = false;
	}

	public static void SyncWindowOptionFromSystem()
	{
		bool flag = !Screen.fullScreen;
		if (_graphicWindowContorl.Value != flag)
		{
			_graphicWindowContorl.Value = flag;
			if (!Screen.fullScreen)
			{
				_IsWindow.Value = true;
			}
			else
			{
				_IsWindow.Value = false;
			}
			ApplyWindowSetting();
		}
	}

	public static void SprkRestoreSavedSettings()
	{
		if (!_sprkSettingsRestored)
		{
			Init();
			_sprkSettingsRestored = true;
		}
	}
}
