using NGame2.NFunctionality;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class SystemButtonAuto : BaseWindowUpdater, IAutoBattle
{
	public interface Listener
	{
		void OnChangeAuto();
	}

	public enum EAutoType
	{
		Manual,
		AutoSkill,
		AutoSkill_FastSpeed,
		FastSpeed
	}

	public enum ESpeedType
	{
		Normal,
		Fast
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/SystemButtonAuto";

	public UISpriteSelector AutoSpriteSelector;

	public UISpriteSelector SpeedSpriteSelector;

	[SerializeField]
	private UIButton _buttonAuto;

	[SerializeField]
	private UIGame3DWidget _effectAuto;

	[SerializeField]
	private UIButton _buttonSpeed;

	[SerializeField]
	private UIGame3DWidget _effectSpeed;

	private Listener _listener;

	private bool _autoPlayerable;

	private EAutoType _applyAutoType;

	private ESpeedType _applySpeedType;

	private LocalAccountEnum<EAutoType> _autoType = new LocalAccountEnum<EAutoType>(EAutoType.Manual, typeof(SystemButtonAuto).FullName + "_autoType");

	private LocalAccountEnum<ESpeedType> _speedType = new LocalAccountEnum<ESpeedType>(ESpeedType.Normal, typeof(SystemButtonAuto).FullName + "_speedType");

	public bool SkillReserveAuto
	{
		get
		{
			if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
			{
				return SprkConquest.ArenaAuto(original: false, this);
			}
			if (SprkStoryAuto.AutoSkills(GameOptionData.EditorToggleAutoSkill))
			{
				return SprkConquest.ArenaAuto(original: true, this);
			}
			if (!_autoPlayerable)
			{
				return SprkConquest.ArenaAuto(original: false, this);
			}
			return SprkConquest.ArenaAuto(_autoType.Value == EAutoType.AutoSkill, this);
		}
	}

	public bool FastSpeed
	{
		get
		{
			if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
			{
				return false;
			}
			if (GameOptionData.EditorToggleFastSpeed)
			{
				return true;
			}
			if (!_autoPlayerable)
			{
				return false;
			}
			return _speedType.Value == ESpeedType.Fast;
		}
	}

	public EAutoType AutoType => _autoType.Value;

	public ESpeedType SpeedType => _speedType.Value;

	public void Open(Listener listener, int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty, bool autoPlayerable)
	{
		base.Open();
		SetUpdatePeriod(0.25f);
		_listener = listener;
		_autoPlayerable = autoPlayerable;
		UIHelper.EnableGameObject(_buttonAuto, _autoPlayerable);
		ApplyAutoType(EAutoType.Manual, force: true);
		ApplySpeedType(ESpeedType.Normal, force: true);
		AdjustOldTypeToNewType();
		_listener.OnChangeAuto();
	}

	private void AdjustOldTypeToNewType()
	{
		if (_autoType.Value == EAutoType.AutoSkill_FastSpeed || _autoType.Value == EAutoType.FastSpeed)
		{
			_speedType.Value = ESpeedType.Fast;
		}
		if (_autoType.Value == EAutoType.AutoSkill_FastSpeed)
		{
			_autoType.Value = EAutoType.AutoSkill;
		}
		if (_autoType.Value == EAutoType.FastSpeed)
		{
			_autoType.Value = EAutoType.Manual;
		}
	}

	public bool IsAuto()
	{
		return _autoType.Value == EAutoType.AutoSkill;
	}

	public void ChangeAutoUI(bool auto)
	{
		if (auto)
		{
			_autoType.Value = EAutoType.AutoSkill;
		}
		else
		{
			_autoType.Value = EAutoType.Manual;
		}
	}

	public void OnClickAuto()
	{
		if (!_autoPlayerable)
		{
			CenterMessage.Open(LocalizationManager.GetString(Battle.AUTO_BUTTON_DISABLED));
		}
		else if (_listener != null)
		{
			if (_autoType.Value == EAutoType.Manual)
			{
				_autoType.Value = EAutoType.AutoSkill;
			}
			else if (_autoType.Value == EAutoType.AutoSkill)
			{
				_autoType.Value = EAutoType.Manual;
			}
			else
			{
				_autoType.Value = EAutoType.Manual;
			}
			_listener.OnChangeAuto();
		}
	}

	public void OnClickFastSpeed()
	{
		if (!_autoPlayerable)
		{
			CenterMessage.Open(LocalizationManager.GetString(Battle.AUTO_BUTTON_DISABLED));
		}
		else if (_listener != null)
		{
			if (_speedType.Value == ESpeedType.Normal)
			{
				_speedType.Value = ESpeedType.Fast;
			}
			else if (_speedType.Value == ESpeedType.Fast)
			{
				_speedType.Value = ESpeedType.Normal;
			}
			else
			{
				_speedType.Value = ESpeedType.Normal;
			}
		}
	}

	public override void UpdatePeriodically()
	{
		EAutoType autoType = _autoType.Value;
		if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
		{
			autoType = EAutoType.Manual;
		}
		ApplyAutoType(autoType, force: false);
		ESpeedType speedType = _speedType.Value;
		if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
		{
			speedType = ESpeedType.Normal;
		}
		ApplySpeedType(speedType, force: false);
	}

	private void ApplyAutoType(EAutoType autoType, bool force)
	{
		if (!force && (!_autoPlayerable || _applyAutoType == autoType))
		{
			return;
		}
		_applyAutoType = autoType;
		if (AutoSpriteSelector != null)
		{
			AutoSpriteSelector.SelectEnum(_applyAutoType);
		}
		if (!(_effectAuto != null))
		{
			return;
		}
		_effectAuto.ReserveContents();
		if (_applyAutoType != EAutoType.Manual)
		{
			if (!_effectAuto.IsPlaying)
			{
				_effectAuto.Play(null);
			}
		}
		else if (_effectAuto.IsPlaying)
		{
			_effectAuto.Stop(immediatelyClear: true);
		}
	}

	private void ApplySpeedType(ESpeedType speedType, bool force)
	{
		if (!force && (!_autoPlayerable || _applySpeedType == speedType))
		{
			return;
		}
		_applySpeedType = speedType;
		if (SpeedSpriteSelector != null)
		{
			SpeedSpriteSelector.SelectEnum(_applySpeedType);
		}
		if (!(_effectSpeed != null))
		{
			return;
		}
		_effectSpeed.ReserveContents();
		if (_applySpeedType != ESpeedType.Normal)
		{
			if (!_effectSpeed.IsPlaying)
			{
				_effectSpeed.Play(null);
			}
		}
		else if (_effectSpeed.IsPlaying)
		{
			_effectSpeed.Stop(immediatelyClear: true);
		}
	}
}
