using System;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NHelper.NTower;
using NGame2.NUI.NWindow.NTower;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class AdventureNoticeComponent : PortalComponent2
{
	[Serializable]
	public class Stamina
	{
		[SerializeField]
		private UISpriteSelector _staminaSelector;

		[SerializeField]
		private Transform _staminaRoot;

		[SerializeField]
		private UILabel _staminaCountLabel;

		[SerializeField]
		private Transform _staminaRechargeRoot;

		[SerializeField]
		private UILabel _staminaRechargeCountLabel;

		[SerializeField]
		private UILabel _staminaRechargeTimeLabel;

		private StaminaType _currentStaminaType;

		public StaminaType CurrentStaminaType => _currentStaminaType;

		public void Set(StaminaType staminaType, long remainCount, long maxCount)
		{
			_currentStaminaType = staminaType;
			_staminaSelector.SelectEnum(staminaType);
			string StaminaRechargeRemainTime = string.Empty;
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurStaminaRemainTime(_currentStaminaType, out StaminaRechargeRemainTime);
			bool flag = !String_Extension_Method.IsNullOrEmpty(StaminaRechargeRemainTime);
			UIHelper.EnableGameObject(_staminaRoot, !flag);
			UIHelper.EnableGameObject(_staminaRechargeRoot, flag);
			UILabel label = _staminaCountLabel;
			if (flag)
			{
				UIHelper.SetLabelText(_staminaRechargeTimeLabel, StaminaRechargeRemainTime);
				label = _staminaRechargeCountLabel;
			}
			switch (staminaType)
			{
			case StaminaType.PrisonKey:
				UIHelper.SetLabelText(label, $"{remainCount} / {maxCount}");
				break;
			case StaminaType.WorldBossGlobalTicket:
			{
				int num2 = 1;
				if (maxCount >= num2)
				{
					UIHelper.SetLabelText(label, $"{remainCount} / {maxCount}");
				}
				else
				{
					UIHelper.SetLabelText(label, $"{$"{remainCount}"}");
				}
				break;
			}
			default:
			{
				int num = 1;
				if (maxCount > num)
				{
					UIHelper.SetLabelText(label, $"{remainCount} / {maxCount}");
				}
				else
				{
					UIHelper.SetLabelText(label, $"{$"{remainCount}"}");
				}
				break;
			}
			}
		}

		public void SetEnable(bool enable)
		{
			UIHelper.EnableGameObject(_staminaSelector, enable);
			UIHelper.EnableGameObject(_staminaRoot, enable);
			UIHelper.EnableGameObject(_staminaRechargeRoot, enable);
		}
	}

	public const string PrefabAdventureNotice = "Prefab2/Component/Portal/AdventureNoticeComponent";

	public static PrefabAuto<AdventureNoticeComponent> PrefabAutoAdventureNotice = new PrefabAuto<AdventureNoticeComponent>("Prefab2/Component/Portal/AdventureNoticeComponent");

	private string disableText = string.Empty;

	public Stamina StaminaUI;

	public UILabel LabelFloor;

	public StaminaTooltipComponent StaminaTooltipComponent;

	public UILabel LabelRemainTime;

	public Transform RemainTimeRoot;

	private Listener _listener;

	private float _lastRefreshInfoTime;

	public AdventureNoticeData AdventrueNoticeData { get; private set; }

	public void Open(Listener listener, int chapterIndex, int dungeonIndex, string labelKey, string textureKey, AdventureNoticeData adventrueNoticeData)
	{
		if (adventrueNoticeData == null)
		{
			return;
		}
		_listener = listener;
		AdventrueNoticeData = adventrueNoticeData;
		_isLock = false;
		Open(null, chapterIndex, dungeonIndex, labelKey, textureKey);
		bool flag = AdventrueNoticeData.StaminaType != StaminaType.None;
		if (flag)
		{
			SetStamina();
		}
		else
		{
			SetOrdealTowerFloor();
		}
		UIHelper.EnableGameObject(LabelFloor, !flag);
		StaminaUI.SetEnable(flag);
		UIHelper.EnableGameObject(Widget_NewMark, enable: false);
		if (!NGame2.NUI.Common.CheckContentsOpenCondition(AdventrueNoticeData.OpenCondition))
		{
			disableText = AdventrueNoticeData.DisableText;
			OffComponent(isOff: true);
			return;
		}
		if (!NGame2.NUI.Common.IsClearTutorial(AdventrueNoticeData))
		{
			disableText = "TUTORIAL_DISABLE_DES";
			OffComponent(isOff: true);
			return;
		}
		if (!NGame2.NUI.Common.CheckContentsStatus(AdventrueNoticeData))
		{
			disableText = "GUILD_RAID_INFO_BUTTON_CLOSE";
			OffComponent(isOff: true);
			return;
		}
		if (!NGame2.NUI.Common.CheckFirstOpen(AdventrueNoticeData))
		{
			NGame2.NUI.Common.SetFirstOpen(opened: true, AdventrueNoticeData);
			UIHelper.EnableGameObject(Widget_NewMark, enable: true);
		}
		EnableEvent();
		OffComponent(isOff: false);
	}

	public new void Update()
	{
		if (!(Time.time - _lastRefreshInfoTime < 1f))
		{
			_lastRefreshInfoTime = Time.time;
			UpdateStamina();
			UpdateBattleRemainTime();
		}
	}

	private void UpdateStamina()
	{
		if (AdventrueNoticeData != null && AdventrueNoticeData.StaminaType != StaminaType.None)
		{
			SetStamina();
		}
	}

	private void UpdateBattleRemainTime()
	{
		if (AdventrueNoticeData != null)
		{
			SetBattleRemainTime();
		}
	}

	private void EnableEvent()
	{
		if (AdventrueNoticeData.EnableEvent)
		{
			DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.BonusStamina, StaminaUI.CurrentStaminaType.ToString());
			UIHelper.EnableGameObject(Sprite_EventFlag, effectiveEvent != null);
		}
	}

	public override void OpenButton(Listener listener, int chapterIndex, int dungeonIndex, string labelKey, string textureKey)
	{
		base.OpenButton(listener, chapterIndex, dungeonIndex, labelKey, textureKey);
		UIHelper.EnableGameObject(Widget_NewMark, enable: false);
	}

	private void SetOrdealTowerFloor()
	{
		if (AdventrueNoticeData != null)
		{
			int num = 0;
			int result = 0;
			if (AdventrueNoticeData.Action.Length >= 2)
			{
				int.TryParse(AdventrueNoticeData.Action[1], out result);
			}
			int maxFloor = BaseTower.GetMaxFloor(result);
			TowerInfo towerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(result);
			if (towerInfo != null)
			{
				num = towerInfo.CompletedFloor;
			}
			string arg = LocalizationManager.GetString(Tower.FLOOR, num);
			string arg2 = LocalizationManager.GetString(Tower.FLOOR, maxFloor);
			UIHelper.SetLabelText(LabelFloor, $"{arg}/{arg2}");
		}
	}

	private void SetStamina()
	{
		if (AdventrueNoticeData != null)
		{
			StaminaType staminaType = AdventrueNoticeData.StaminaType;
			int num = 0;
			long currentValue = num;
			int num2 = 0;
			long maxValue = num2;
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(staminaType, out currentValue, out maxValue);
			StaminaUI.Set(staminaType, currentValue, maxValue);
		}
	}

	private void SetBattleRemainTime()
	{
		string remainTimeText = GetRemainTimeText(AdventrueNoticeData.BattleType);
		UIHelper.SetLabelText(LabelRemainTime, LocalizationManager.GetString(GuildRaid.GUILDRAID_STATE_OPENED, remainTimeText));
	}

	private string GetRemainTimeText(BattleType battleType)
	{
		_ = DateTime.MinValue;
		int num = 0;
		long num2 = num;
		switch (battleType)
		{
		case BattleType.Labyrinth:
		case BattleType.TreasureHouse:
		case BattleType.GodkingTrial:
		case BattleType.Eclipse:
		case BattleType.UnderPrison:
			return LocalizationManager.GetStringDirect("ADVENTURE_NOTICE_STAMINA_ETC");
		case BattleType.ChallengeTower:
		{
			int num5 = Mathf.Max(TowerHelper.GetRemainResetTime(GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Challenge)), 0);
			num2 = num5;
			break;
		}
		case BattleType.OrdealTower:
		{
			int num3 = Mathf.Max(TowerHelper.GetRemainResetTime(GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Ordeal)), 0);
			num2 = num3;
			break;
		}
		case BattleType.Maze:
		{
			int num4 = Mathf.Max(TowerHelper.GetRemainResetTime(GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Maze)), 0);
			num2 = num4;
			break;
		}
		case BattleType.Arena:
		case BattleType.WorldBoss:
		case BattleType.GuildRaid:
		case BattleType.ArenaBanPick:
		case BattleType.GuildSuppress:
			if (GlobalAutoBehaviour<AccountManager>.instance.AdventureNoticeManager != null)
			{
				UserAdventureNoticeManager.AdventureNoticeInfo adventureNoticeInfo = GlobalAutoBehaviour<AccountManager>.instance.AdventureNoticeManager.GetAdventureNoticeInfo(battleType);
				if (adventureNoticeInfo != null)
				{
					bool useLocalTime = AdventrueNoticeData != null && AdventrueNoticeData.UseLocalTime;
					int remainTime = adventureNoticeInfo.GetRemainTime(useLocalTime);
					num2 = remainTime;
				}
			}
			break;
		}
		if (num2 == 0L)
		{
			return "-";
		}
		return UIHelper.GetRemainTimeLabelText(num2);
	}

	public void OnClickContentsShortCut()
	{
		if (!_isLock && _listener != null)
		{
			if (Selectable())
			{
				_listener.SelectEnableContentsShortCut(this);
			}
			else
			{
				_listener.SelectDisableContentsShortCut(disableText);
			}
		}
	}

	public void OnShowStaminaTooltip()
	{
		if (AdventrueNoticeData != null && AdventrueNoticeData.StaminaType != StaminaType.None && !(StaminaTooltipComponent == null))
		{
			StaminaTooltipComponent.OnShowTooltip(StaminaUI.CurrentStaminaType);
		}
	}

	public void OnHideStaminaTooltip()
	{
		if (!(StaminaTooltipComponent == null))
		{
			StaminaTooltipComponent.OnHideTooltip();
		}
	}
}
