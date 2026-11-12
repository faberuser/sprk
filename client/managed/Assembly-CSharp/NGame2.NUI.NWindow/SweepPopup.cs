using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SweepPopup : BaseWindow, CountInputField2.IListener
{
	public enum SweepPopupUIType
	{
		None,
		Tower,
		MiddleShakmeh,
		Shakmeh,
		Eclipse
	}

	[Serializable]
	public class SweepPanel
	{
		public SweepPopupUIType SweepPopupType;

		public Transform Root;

		public UILabel MessageLabel;

		public Transform SweepStatusRoot;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/SweepPopup";

	[Header("Fixed UI")]
	public UILabel TitleLabel;

	public UIBaseGrid IconGrid;

	public UIButton OkButton;

	public CountInputField2 CountInputField;

	public GameObject GuideTooltip;

	public UILabel GuideTooltipText;

	[Header("Special UI")]
	public List<SweepPanel> SweepPanelsByType;

	private SweepPopupUIType _curPanelType;

	private SweepPanel _curPanel;

	private CampaignDungeonData _dungeonData;

	private CampaignDungeonData _topCleardungeonData;

	private StaminaType _reqStaminaType;

	private int _reqStaminaCount;

	private int _reqTicketCount;

	private List<Component> _compList = new List<Component>();

	private ShakmehGaugeComponent _shakmehGaugeComponent;

	private TowerFloorStatusComponent _towerFloorStatusComponent;

	private ISweepDungeonWindow _sweepWindow;

	public string GetGuideLocalText(SweepDungeonType dungeonType)
	{
		string empty = string.Empty;
		string format = "SWEEP_TOOLTIP_{0}";
		empty = ((dungeonType != SweepDungeonType.MazeArcher && dungeonType != SweepDungeonType.MazeAssassin && dungeonType != SweepDungeonType.MazeKnight && dungeonType != SweepDungeonType.MazeMechanic && dungeonType != SweepDungeonType.MazePriest && dungeonType != SweepDungeonType.MazeWarrior && dungeonType != SweepDungeonType.MazeWizard) ? (empty = string.Format(format, dungeonType.ToString().ToUpper())) : string.Format(format, "MAZE"));
		return LocalizationManager.GetStringDirect(empty);
	}

	public SweepPopupUIType GetSweepPopupUIType(SweepDungeonType sweepDungeonType)
	{
		SweepPopupUIType result = SweepPopupUIType.None;
		switch (sweepDungeonType)
		{
		case SweepDungeonType.ChallengeTower:
		case SweepDungeonType.MazeKnight:
		case SweepDungeonType.MazeWarrior:
		case SweepDungeonType.MazeAssassin:
		case SweepDungeonType.MazeArcher:
		case SweepDungeonType.MazeMechanic:
		case SweepDungeonType.MazeWizard:
		case SweepDungeonType.MazePriest:
			result = SweepPopupUIType.Tower;
			break;
		case SweepDungeonType.Eclipse:
			result = SweepPopupUIType.Eclipse;
			break;
		case SweepDungeonType.Shakmeh:
			result = SweepPopupUIType.Shakmeh;
			break;
		case SweepDungeonType.MiddleShakmeh:
			result = SweepPopupUIType.MiddleShakmeh;
			break;
		}
		return result;
	}

	public static void Open(CampaignDungeonData dungeonData, ISweepDungeonWindow sweepWindow)
	{
		WindowPrefab<SweepPopup> windowPrefab = new WindowPrefab<SweepPopup>("Resources/Prefab2/Window/MessagePopup/SweepPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(dungeonData, sweepWindow);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(CampaignDungeonData dungeonData, ISweepDungeonWindow sweepWindow)
	{
		base.Open();
		if (dungeonData.SweepDungeonType != SweepDungeonType.None)
		{
			_dungeonData = dungeonData;
			_topCleardungeonData = SweepHelper.GetTopClearDungeonData(dungeonData);
			_sweepWindow = sweepWindow;
			SweepHelper.GetSweepReqItemInfo(dungeonData, ref _reqStaminaType, ref _reqStaminaCount, ref _reqTicketCount);
			Init();
		}
	}

	private void Init()
	{
		InitCountInputField();
	}

	private void InitCountInputField()
	{
		if (!(CountInputField == null))
		{
			UIHelper.EnableGameObject(CountInputField, enable: true);
			long possibleSweepMaxCount = SweepHelper.GetPossibleSweepMaxCount(_topCleardungeonData);
			CountInputField2 countInputField = CountInputField;
			CountInputField2.CountInputFieldInfo countInputFieldInfo = new CountInputField2.CountInputFieldInfo();
			int num = 0;
			countInputFieldInfo.MinCount = ((possibleSweepMaxCount > num) ? 1 : 0);
			countInputFieldInfo.MaxCount = possibleSweepMaxCount;
			int num2 = 0;
			int num3 = ((possibleSweepMaxCount > num2) ? 1 : 0);
			countInputFieldInfo.InitCount = num3;
			countInputField.Open(this, countInputFieldInfo);
		}
	}

	private void RefreshUI()
	{
		SetSweepReqItemComp();
		SetSpecialUIByDungeon();
	}

	private void SetSweepReqItemComp()
	{
		if (_dungeonData.SweepDungeonType == SweepDungeonType.None)
		{
			return;
		}
		ReleaseTicketComp();
		long currentCount = CountInputField.CurrentCount;
		checked
		{
			if (_reqTicketCount != 0)
			{
				int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("SweepTicketItemIndex");
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(valueInt);
				int reqTicketCount = _reqTicketCount;
				long num = currentCount * reqTicketCount;
				int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data.Index);
				long num2 = itemCount;
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(IconGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(data);
				itemIconComponent.SetUnderLabel(displayUnderLabel: true, $"{num}/{num2}", isLightBackground: false);
				_compList.Add(itemIconComponent);
			}
			if (_reqStaminaCount != 0)
			{
				CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(IconGrid, CurrencyIconComponent.PrefabAuto);
				StaminaInfo staminaInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetStaminaInfo(_reqStaminaType);
				CurrencyType currencyTypeByStaminaType = SweepHelper.GetCurrencyTypeByStaminaType(_reqStaminaType);
				int reqStaminaCount = _reqStaminaCount;
				long num = currentCount * reqStaminaCount;
				long num3;
				if (staminaInfo != null)
				{
					num3 = staminaInfo.CurValue;
				}
				else
				{
					int num4 = 0;
					num3 = num4;
				}
				long num2 = num3;
				CurrencyIconComponent.Listener listener = null;
				int num5 = 0;
				currencyIconComponent.Open(currencyTypeByStaminaType, num5, listener);
				currencyIconComponent.SetUnderLabel($"{num}/{num2}");
				_compList.Add(currencyIconComponent);
			}
			UIHelper.RepositionGrid(IconGrid);
		}
	}

	private void SetSpecialUIByDungeon()
	{
		_curPanelType = GetSweepPopupUIType(_dungeonData.SweepDungeonType);
		_curPanel = OnOffSweepPanel(_curPanelType);
		if (_curPanel != null)
		{
			switch (_curPanelType)
			{
			case SweepPopupUIType.Tower:
				SetTowerFloor(_curPanel.SweepStatusRoot);
				break;
			case SweepPopupUIType.MiddleShakmeh:
				SetShakmehGauage(_curPanel.SweepStatusRoot);
				break;
			case SweepPopupUIType.Shakmeh:
				UIHelper.EnableGameObject(CountInputField, enable: false);
				break;
			case SweepPopupUIType.Eclipse:
				break;
			}
		}
	}

	private void SetTowerFloor(Transform towerFloorStatusRoot)
	{
		TowerFloorData towerDataByDungeon = GameRoot.Instance.TowerFloorDataContainer.GetTowerDataByDungeon(_topCleardungeonData.ChapterIndex, _topCleardungeonData.DungeonIndex);
		if (towerDataByDungeon != null)
		{
			if (_towerFloorStatusComponent == null)
			{
				_towerFloorStatusComponent = TowerFloorStatusComponent.PrefabAuto.Factory.Create(towerFloorStatusRoot);
			}
			long currentCount = CountInputField.CurrentCount;
			if (currentCount == 0L)
			{
				TowerFloorStatusComponent towerFloorStatusComponent = _towerFloorStatusComponent;
				SweepDungeonType sweepDungeonType = _topCleardungeonData.SweepDungeonType;
				int num = 0;
				int topStep = 0;
				int num2 = 0;
				towerFloorStatusComponent.Open(sweepDungeonType, 0, num2, topStep, num);
			}
			else
			{
				TowerInfo towerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(towerDataByDungeon.TowerIndex);
				int num3 = ((towerInfo == null) ? 1 : (towerInfo.CompletedFloor + 1));
				int num4 = num3 - 1;
				long num5 = currentCount;
				int num6 = num4;
				long num7 = checked(num6 + num5);
				long val = num7;
				int floor = towerDataByDungeon.Floor;
				num7 = Math.Min(val, floor);
				_towerFloorStatusComponent.Open(_topCleardungeonData.SweepDungeonType, num3, num7, towerDataByDungeon.Floor, currentCount);
			}
		}
	}

	private void SetShakmehGauage(Transform ShakmehGaugeRoot)
	{
		if (_shakmehGaugeComponent == null)
		{
			_shakmehGaugeComponent = ShakmehGaugeComponent.PrefabAuto.Factory.Create(ShakmehGaugeRoot);
		}
		long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);
		int expectedBossPointByDungeon = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance.GetExpectedBossPointByDungeon(_topCleardungeonData.DungeonIndex);
		long currentCount = CountInputField.CurrentCount;
		int num = expectedBossPointByDungeon;
		long num2 = checked(userStat + currentCount * num);
		_ = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.ShakmehMiddleBossPoint).MaxValue;
		_shakmehGaugeComponent.Open(userStat, userStat, 0f, autoStart: true);
		_shakmehGaugeComponent.SetExpectedGauage(num2);
	}

	private SweepPanel OnOffSweepPanel(SweepPopupUIType sweepPopupType)
	{
		SweepPanel result = null;
		bool flag = false;
		foreach (SweepPanel item in SweepPanelsByType)
		{
			flag = sweepPopupType == item.SweepPopupType;
			if (flag)
			{
				result = item;
			}
			UIHelper.EnableGameObject(item.Root, flag);
		}
		return result;
	}

	private void OnSuccessSweep()
	{
		if (_sweepWindow != null)
		{
			_sweepWindow.RefreshBySweep();
		}
	}

	public override void Reset()
	{
		base.Reset();
		ReleaseTicketComp();
		ReleaseSpecialComp();
	}

	private void ReleaseTicketComp()
	{
		FactoryBase.ReleaseAndClearList(_compList);
	}

	private void ReleaseSpecialComp()
	{
		FactoryBase.SafeReleaseComponent(ref _shakmehGaugeComponent);
		FactoryBase.SafeReleaseComponent(ref _towerFloorStatusComponent);
	}

	void CountInputField2.IListener.ChangeCount()
	{
		RefreshUI();
	}

	public void OnClickSweep()
	{
		CampaignDungeonData sweepDungeonData = _dungeonData;
		SweepDungeonType sweepDungeonType = _dungeonData.SweepDungeonType;
		if ((uint)(sweepDungeonType - 9) <= 1u)
		{
			sweepDungeonData = _topCleardungeonData;
		}
		if (SweepHelper.IsPreSelectRewardDungeon(sweepDungeonData.SweepDungeonType))
		{
			SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(sweepDungeonData.ChapterIndex, sweepDungeonData.DungeonIndex);
			PreSelectRewardPopup.Open(sweepDungeonData.BattleType, data, delegate(List<string> rewardList)
			{
				SweepHelper.RequestSweep(sweepDungeonData, CountInputField.CurrentCount, rewardList.ToArray(), delegate
				{
					Close();
					OnSuccessSweep();
				});
			});
		}
		else
		{
			SweepHelper.RequestSweep(sweepDungeonData, CountInputField.CurrentCount, null, delegate
			{
				Close();
				OnSuccessSweep();
			});
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnShowGuideTooltip()
	{
		string guideLocalText = GetGuideLocalText(_topCleardungeonData.SweepDungeonType);
		UIHelper.SetLabelText(GuideTooltipText, guideLocalText);
		UIHelper.EnableGameObject(GuideTooltip, enable: true);
	}

	public void OnHideGuideTooltip()
	{
		UIHelper.EnableGameObject(GuideTooltip, enable: false);
	}
}
