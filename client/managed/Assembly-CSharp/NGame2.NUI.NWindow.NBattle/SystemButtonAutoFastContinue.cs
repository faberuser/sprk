using System;
using NGame2.NBattleContext;
using NGame2.NFunctionality;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class SystemButtonAutoFastContinue : BaseWindow, IAutoBattle
{
	public interface Listener
	{
		void OnChangeAuto();

		void RequestContinuousBattle(bool isContinuous, Action<bool> onSuccess);
	}

	public enum EAutoType
	{
		Manual,
		AutoSkill
	}

	public enum EContinueType
	{
		Normal,
		Continue
	}

	public class SystemAutoFastContinueInfo
	{
		public Listener Listener { get; set; }

		public int ChapterIndex { get; set; }

		public int DungeonIndex { get; set; }

		public NShared.DungeonDifficultyType Difficulty { get; set; }

		public bool AutoPlayerable { get; set; }

		public int GameSpeed { get; set; }

		public int MaxGameSpeed { get; set; }

		public bool IsPossibleChangeGameSpeed { get; set; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/SystemButtonAutoFastContinue";

	public UISpriteSelector AutoSpriteSelector;

	public UISpriteSelector SpeedSpriteSelector;

	public UISpriteSelector ContinueSpriteSelector;

	[SerializeField]
	private UIButton _buttonAuto;

	[SerializeField]
	private UIGame3DWidget _effectAuto;

	[SerializeField]
	private UIButton _buttonSpeed;

	[SerializeField]
	private UIGame3DWidget _effectSpeed;

	[SerializeField]
	private UIButton _buttonContinue;

	[SerializeField]
	private UIGame3DWidget _effectContinue;

	private const int DefaultGameSpeed = 1;

	private SystemAutoFastContinueInfo _info;

	private bool _isInit;

	private EAutoType _applyAutoType;

	private EContinueType _applyContinueType;

	private int _gameSpeed = 1;

	private LocalAccountEnum<EAutoType> _autoType;

	private LocalAccountInt _localAccountGameSpeed = new LocalAccountInt(1, typeof(SystemButtonAutoFastContinue).FullName + "_GAME_SPEED");

	private bool _isPendingRequestContinousBattle;

	public bool SkillReserveAuto
	{
		get
		{
			if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
			{
				return false;
			}
			if (SprkStoryAuto.AutoSkills(GameOptionData.EditorToggleAutoSkill))
			{
				return true;
			}
			if (_info == null)
			{
				return false;
			}
			if (!_info.AutoPlayerable)
			{
				return false;
			}
			return _autoType.Value == EAutoType.AutoSkill;
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
			if (_info == null)
			{
				return false;
			}
			if (!_info.AutoPlayerable)
			{
				return false;
			}
			return _localAccountGameSpeed.Value > 1;
		}
	}

	public bool Continue
	{
		get
		{
			if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
			{
				return false;
			}
			if (_info == null)
			{
				return false;
			}
			if (!_info.AutoPlayerable)
			{
				return false;
			}
			return SingletonAuto<BattleContext>.instance.ContinueType.Value == EContinueType.Continue;
		}
	}

	public void Open(SystemAutoFastContinueInfo info)
	{
		if (info != null)
		{
			base.Open();
			string i_key = typeof(SystemButtonAutoFastContinue).FullName + "_AUTO_TYPE_" + info.GameSpeed + "_200609_0527";
			_autoType = new LocalAccountEnum<EAutoType>(EAutoType.Manual, i_key);
			_info = info;
			_isInit = true;
			LocalAccountInt localAccountGameSpeed = _localAccountGameSpeed;
			int gameSpeed = info.GameSpeed;
			localAccountGameSpeed.Value = gameSpeed;
			_isPendingRequestContinousBattle = false;
			UIHelper.EnableGameObject(_buttonAuto, info.AutoPlayerable);
			UIHelper.EnableButton(_buttonSpeed, info.IsPossibleChangeGameSpeed);
			ApplyAutoType(_autoType.Value, force: true);
			ApplySpeedType(info.GameSpeed, force: true);
			ApplyContinueType(EContinueType.Normal, force: true);
			if (info.Listener != null)
			{
				info.Listener.OnChangeAuto();
			}
		}
	}

	public bool IsInitAutoButton()
	{
		return _isInit;
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
		if (_info == null)
		{
			return;
		}
		if (!_info.AutoPlayerable)
		{
			CenterMessage.Open(LocalizationManager.GetString(Battle.AUTO_BUTTON_DISABLED));
		}
		else if (_info.Listener != null)
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
			_info.Listener.OnChangeAuto();
		}
	}

	public void OnClickFastSpeed()
	{
		if (_info == null || !_info.IsPossibleChangeGameSpeed)
		{
			return;
		}
		if (!_info.AutoPlayerable)
		{
			CenterMessage.Open(LocalizationManager.GetString(Battle.AUTO_BUTTON_DISABLED));
		}
		else if (_info.Listener != null)
		{
			int value = _localAccountGameSpeed.Value;
			long num = value;
			long num2 = num;
			int num3 = 1;
			num = checked(num2 + num3);
			int maxGameSpeed = _info.MaxGameSpeed;
			long num4 = num;
			int num5 = maxGameSpeed;
			if (num5 < num4)
			{
				int num6 = 1;
				num = num6;
			}
			_localAccountGameSpeed.Value = checked((int)num);
		}
	}

	public void OnClickContinue()
	{
		if (_info != null && _info.Listener != null)
		{
			LocalAccountEnum<EContinueType> continueType = SingletonAuto<BattleContext>.instance.ContinueType;
			SetContionuousBattle(continueType.Value == EContinueType.Normal);
		}
	}

	private void SetContionuousBattle(bool isContinuous)
	{
		if (_isPendingRequestContinousBattle)
		{
			return;
		}
		_info.Listener.RequestContinuousBattle(isContinuous, delegate(bool success)
		{
			_isPendingRequestContinousBattle = false;
			if (success)
			{
				SingletonAuto<BattleContext>.instance.ContinueType.Value = (isContinuous ? EContinueType.Continue : EContinueType.Normal);
			}
		});
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		EAutoType autoType = _autoType.Value;
		if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
		{
			autoType = EAutoType.Manual;
		}
		ApplyAutoType(autoType, force: false);
		int gameSpeed = _localAccountGameSpeed.Value;
		if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
		{
			gameSpeed = 1;
		}
		ApplySpeedType(gameSpeed, force: false);
		EContinueType continueType = SingletonAuto<BattleContext>.instance.ContinueType.Value;
		if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
		{
			continueType = EContinueType.Normal;
		}
		ApplyContinueType(continueType, force: false);
	}

	private void ApplyAutoType(EAutoType autoType, bool force)
	{
		if (_info == null || (!force && (!_info.AutoPlayerable || _applyAutoType == autoType)))
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

	private void ApplySpeedType(int gameSpeed, bool force)
	{
		if (_info == null || (!force && (!_info.AutoPlayerable || _gameSpeed == gameSpeed)))
		{
			return;
		}
		_gameSpeed = gameSpeed;
		if (SpeedSpriteSelector != null)
		{
			SpeedSpriteSelector.Select(_gameSpeed.ToString());
		}
		if (_effectSpeed != null)
		{
			_effectSpeed.ReserveContents();
			if (!_effectSpeed.IsPlaying)
			{
				_effectSpeed.Play(null);
			}
		}
	}

	private void ApplyContinueType(EContinueType continueType, bool force)
	{
		if (_info == null || (!force && (!_info.AutoPlayerable || _applyContinueType == continueType)))
		{
			return;
		}
		_applyContinueType = continueType;
		if (ContinueSpriteSelector != null)
		{
			ContinueSpriteSelector.SelectEnum(_applyContinueType);
		}
		if (!(_effectContinue != null))
		{
			return;
		}
		_effectContinue.ReserveContents();
		if (_applyContinueType != EContinueType.Normal)
		{
			if (!_effectContinue.IsPlaying)
			{
				_effectContinue.Play(null);
			}
		}
		else if (_effectContinue.IsPlaying)
		{
			_effectContinue.Stop(immediatelyClear: true);
		}
	}
}
