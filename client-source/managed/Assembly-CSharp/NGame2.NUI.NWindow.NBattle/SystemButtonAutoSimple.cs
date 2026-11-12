using NGame2.NFunctionality;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class SystemButtonAutoSimple : BaseWindow, IAutoBattle
{
	public interface Listener
	{
		void OnChangeAuto();
	}

	public enum EAutoType
	{
		Manual,
		AutoSkill
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/SystemButtonAutoSimple";

	public UISpriteSelector AutoSpriteSelector;

	[SerializeField]
	private UIButton _buttonAuto;

	[SerializeField]
	private UIGame3DWidget _effectAuto;

	private BattleType _battleType;

	private Listener _listener;

	private bool _autoPlayerable;

	private EAutoType _applyAutoType;

	public GameObject Offset_Arena;

	private bool _isInit;

	private LocalAccountEnum<EAutoType> _autoType = new LocalAccountEnum<EAutoType>(EAutoType.Manual, typeof(SystemButtonAutoSimple).FullName + "_autoType");

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

	public void Open(Listener listener, int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty, bool autoPlayerable)
	{
		base.Open();
		_listener = listener;
		_autoPlayerable = autoPlayerable;
		_isInit = true;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			_battleType = data.BattleType;
			string i_key = string.Format("{0}{1}{2}", typeof(SystemButtonAutoSimple).FullName, "_autoType_", _battleType.ToString());
			if (_battleType == BattleType.Arena || _battleType == BattleType.GuildArena || _battleType == BattleType.Raid || _battleType == BattleType.ChallengeRaid || _battleType == BattleType.HardRaid || _battleType == BattleType.GuildSuppress || _battleType == BattleType.Eclipse || _battleType == BattleType.PunishmentRaid)
			{
				_autoType = new LocalAccountEnum<EAutoType>(EAutoType.Manual, i_key);
			}
		}
		SetTransformToRaidOffset();
		UIHelper.EnableGameObject(_buttonAuto, _autoPlayerable);
		ApplyAutoType(EAutoType.Manual, force: true);
		_listener.OnChangeAuto();
	}

	public bool IsInitAutoButton()
	{
		return _isInit;
	}

	public void SetTransformToRaidOffset()
	{
		if (!(Offset_Arena == null) && (_battleType == BattleType.Arena || _battleType == BattleType.GuildArena || _battleType == BattleType.ArenaBanPick))
		{
			_buttonAuto.transform.localPosition = Offset_Arena.transform.localPosition;
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
			else
			{
				_autoType.Value = EAutoType.Manual;
			}
			_listener.OnChangeAuto();
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

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		EAutoType autoType = _autoType.Value;
		if (GlobalAutoBehaviour<FunctionalityManager>.isValidInstance && GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.DisableAutoBattle, value: true))
		{
			autoType = EAutoType.Manual;
		}
		ApplyAutoType(autoType, force: false);
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
}
