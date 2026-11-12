using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class TreasureHouseWindow : BaseWindow, TreasureHouseBossComponent.IListener, IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>
{
	public interface Listener
	{
		void OnClickEnterDungeon(int chapterIndex, int dungeonIndex);

		void OnClickChangeBoss(CurrencyType currencyType, int price);

		void RefreshTreasureHouseInfo(Action OnSuccess);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/TreasureHouse/TreasureHouseWindow";

	public UISimpleGrid SelectBossGrid;

	public UIScrollGrid BossTabGrid;

	public UISimpleGrid RewardItemGrid;

	public UIWidget DescTooltipWidget;

	public UILabel DescTooltipLabel;

	public UIProgressBar GaugeBar;

	public UILabel RemainTimeLabel;

	public UIWidget CheckSelectBossLabelWidget;

	public UILabel CheckSelectBossLabel;

	public UIWidget ClearLabelWidget;

	public UIWidget RewardWidget;

	public UISprite ResetButton;

	public UIButton EnterDungeonButton;

	public UISimpleGrid CreatureWidgetGrid;

	public UIGame3DWidget[] SelectEffectWidgets;

	public int CreatureWidgetCompDepth = 2;

	private Dictionary<int, CreatureWidgetComponent> _creatureWidgetComponentDic = new Dictionary<int, CreatureWidgetComponent>();

	private TreasureHouseBossComponent _selectBossComponent;

	private List<TreasureHouseBossComponent> _treasureHouseBossComponentList = new List<TreasureHouseBossComponent>();

	private List<ItemIconComponent> _itemIconComponentList = new List<ItemIconComponent>();

	private List<CurrencyIconComponent> _currencyIconComponentList = new List<CurrencyIconComponent>();

	private PlayerTreasureHouseInfo _playerTreasureHouseInfo;

	private TreasureHouseDungeonData _playerTreasureHouseDungeonData;

	private CreatureWidgetComponent _currentCreatureWidget;

	private Listener _listener;

	public void Open(Listener listener)
	{
		_listener = listener;
		base.Open(delegate
		{
			InitCreatureWidgetComponent();
			if (_playerTreasureHouseDungeonData != null)
			{
				SelectCreatureWidget(_playerTreasureHouseDungeonData.Index);
			}
		});
		if (InitTreasureHouseData())
		{
			InitTreasureHouse();
		}
	}

	private void InitCreatureWidgetComponent()
	{
		TreasureHouseDungeonData[] array = GameRoot.Instance.TreasureHouseDungeonDataContainer.GetAllData().ToArray();
		Array.Sort(array, (TreasureHouseDungeonData lhs, TreasureHouseDungeonData rhs) => lhs.SequenceIndex.CompareTo(rhs.SequenceIndex));
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(array); num++)
		{
			TreasureHouseDungeonData treasureHouseDungeonData = array[num];
			if (treasureHouseDungeonData != null)
			{
				CreatureWidgetComponent creatureWidgetComponent = UIHelper.AddGridItem(CreatureWidgetGrid, CreatureWidgetComponent.PrefabAuto);
				if (!(creatureWidgetComponent == null))
				{
					creatureWidgetComponent.Open(treasureHouseDungeonData.CreatureIndex, treasureHouseDungeonData.CreaturePosition, treasureHouseDungeonData.CreatureRotation, treasureHouseDungeonData.CreatureScale);
					creatureWidgetComponent.SetDepth(CreatureWidgetCompDepth);
					_creatureWidgetComponentDic.Add(treasureHouseDungeonData.Index, creatureWidgetComponent);
				}
			}
		}
	}

	private void RelaseCreatureWidgetComp()
	{
		foreach (KeyValuePair<int, CreatureWidgetComponent> item in _creatureWidgetComponentDic)
		{
			CreatureWidgetComponent value = item.Value;
			if (!(value == null))
			{
				FactoryBase.ReleaseComponent(value);
			}
		}
		_creatureWidgetComponentDic.Clear();
	}

	private void SelectCreatureWidget(int index)
	{
		if (_currentCreatureWidget != null)
		{
			_currentCreatureWidget.SetAlpha(0f);
		}
		if (_creatureWidgetComponentDic.TryGetValue(index, out _currentCreatureWidget))
		{
			_currentCreatureWidget.SetAlpha(1f);
		}
	}

	private bool InitTreasureHouseData()
	{
		_playerTreasureHouseInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TreasureHouseInfo;
		if (_playerTreasureHouseInfo == null)
		{
			VespaDebug.LogError("PlayerTreasureHouseInfo is null");
			return false;
		}
		_playerTreasureHouseDungeonData = GameRoot.Instance.TreasureHouseDungeonDataContainer.GetData(_playerTreasureHouseInfo.Index);
		if (_playerTreasureHouseDungeonData == null)
		{
			VespaDebug.LogError("playerTreasureHouseData is null");
			return false;
		}
		return true;
	}

	private void InitTreasureHouse()
	{
		SetSelectBossComponent(_playerTreasureHouseDungeonData);
		SetBossList(_playerTreasureHouseDungeonData.InfoIndex);
		SetBossInfo(_playerTreasureHouseDungeonData);
		SetGauge();
		SetResetButton();
		SetEnterDungeonButton();
		SetTooltipDesc();
		SetClearLabel();
	}

	private void SetClearLabel()
	{
		UIHelper.EnableGameObject(ClearLabelWidget, CheckDungeonCleared());
	}

	private void SetTooltipDesc()
	{
		TreasureHouseInfoData data = GameRoot.Instance.TreasureHouseInfoDataContainer.GetData(_playerTreasureHouseDungeonData.InfoIndex);
		if (data == null)
		{
			VespaDebug.LogError("treasureHouseInfoData (TableData) is null");
		}
		else
		{
			UIHelper.SetLabelText(DescTooltipLabel, LocalizationManager.GetStringDirect(data.ContentsDesc));
		}
	}

	private void SetSelectBossComponent(TreasureHouseDungeonData treasureHousedungeonData)
	{
		FactoryBase.SafeReleaseComponent(ref _selectBossComponent);
		TreasureHouseBossComponent treasureHouseBossComponent = UIHelper.AddGridItem(SelectBossGrid, TreasureHouseBossComponent.PrefabAuto2);
		treasureHouseBossComponent.Open(this, treasureHousedungeonData, isSelectedDungeon: true);
		treasureHouseBossComponent.SelectToggle(isSelect: true);
		_selectBossComponent = treasureHouseBossComponent;
	}

	private void SetBossList(int infoIndex)
	{
		FactoryBase.ReleaseAndClearList(_treasureHouseBossComponentList);
		int num = 1;
		while (true)
		{
			TreasureHouseDungeonData data = GameRoot.Instance.TreasureHouseDungeonDataContainer.GetData(infoIndex, num);
			if (data == null)
			{
				break;
			}
			num++;
			if (_playerTreasureHouseDungeonData.Index != data.Index)
			{
				TreasureHouseBossComponent treasureHouseBossComponent = UIHelper.AddGridItem(BossTabGrid, TreasureHouseBossComponent.PrefabAuto);
				treasureHouseBossComponent.Open(this, data);
				_treasureHouseBossComponentList.Add(treasureHouseBossComponent);
			}
		}
		UIHelper.RepositionGrid(BossTabGrid);
	}

	private void SetGauge()
	{
		if (GaugeBar == null)
		{
			return;
		}
		CurrencyTypeData currencyData = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(CurrencyType.TreasureHousePoint);
		if (currencyData == null)
		{
			return;
		}
		if (CheckDungeonCleared())
		{
			GaugeBar.value = 0f;
			SetReaminText(LocalizationManager.GetStringDirect("TREASUREHOUSE_GAUGE_ UNABLE_CHARGE"));
			return;
		}
		int maxValue = currencyData.MaxValue;
		long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.TreasureHousePoint);
		float value = (float)userStat / (float)maxValue;
		long remainTime = checked(maxValue - userStat);
		if (maxValue <= userStat)
		{
			GaugeBar.value = 1f;
			SetReaminText(LocalizationManager.GetStringDirect("TREASUREHOUSE_GAUGE_ENTER"));
			return;
		}
		GaugeBar.value = value;
		string stringDirect = LocalizationManager.GetStringDirect("TREASUREHOUSE_GAUGE_CHARGE", UIHelper.GetRemainTimeTextAll(remainTime));
		SetReaminText(stringDirect);
	}

	public void SetReaminText(string text)
	{
		UIHelper.SetLabelText(RemainTimeLabel, text);
	}

	private void SetBossInfo(TreasureHouseDungeonData treasureHousedungeonData)
	{
		bool flag = treasureHousedungeonData.Index == _playerTreasureHouseDungeonData.Index;
		SetCheckSelectBossLabelEnable(!flag);
		SetRewards(treasureHousedungeonData.RewardIndex);
		SelectCreatureWidget(treasureHousedungeonData.Index);
	}

	private void SetCheckSelectBossLabelEnable(bool isEnable)
	{
		if (CheckDungeonCleared())
		{
			isEnable = false;
		}
		UIHelper.EnableGameObject(CheckSelectBossLabelWidget, isEnable);
		if (isEnable)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_playerTreasureHouseDungeonData.CreatureIndex);
			if (data != null)
			{
				UIHelper.SetLabelText(CheckSelectBossLabel, LocalizationManager.GetStringDirect("TREASUREHOUSE_MONSTER_APPEAR_INFO", data.Name));
			}
		}
	}

	private void SetRewards(int rewardIndex)
	{
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
		FactoryBase.ReleaseAndClearList(_currencyIconComponentList);
		UIHelper.EnableGameObject(RewardWidget, enable: true);
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data != null)
		{
			foreach (RewardElementData rewardItem in data.GetRewardItemList())
			{
				if (__Extension_Method_ICollection__.COUNT(_currencyIconComponentList) >= 2)
				{
					break;
				}
				if (data == null)
				{
					continue;
				}
				CurrencyType currencyType = rewardItem.GetCurrencyType();
				if (currencyType != CurrencyType.None)
				{
					CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(RewardItemGrid, CurrencyIconComponent.PrefabAuto);
					int itemMin = rewardItem.ItemMin;
					CurrencyIconComponent.Listener listener = null;
					int num = itemMin;
					currencyIconComponent.Open(currencyType, num, listener);
					_currencyIconComponentList.Add(currencyIconComponent);
					continue;
				}
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardItem.ItemCode);
				if (dataByCode != null)
				{
					int itemMin2 = rewardItem.ItemMin;
					long count = itemMin2;
					int itemMax = rewardItem.ItemMax;
					long maxCount = itemMax;
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardItemGrid, ItemIconComponent.PrefabAuto);
					itemIconComponent.Open(dataByCode);
					if (dataByCode.Type != ItemType.Equip)
					{
						itemIconComponent.SetItemMinMax(count, maxCount);
					}
					_itemIconComponentList.Add(itemIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(RewardItemGrid);
	}

	private void SetEnterDungeonButton()
	{
		if (CheckDungeonCleared())
		{
			UIHelper.EnableButton(EnterDungeonButton, enable: false);
			return;
		}
		CurrencyTypeData currencyData = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(CurrencyType.TreasureHousePoint);
		if (currencyData != null)
		{
			int maxValue = currencyData.MaxValue;
			long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.TreasureHousePoint);
			UIHelper.EnableButton(EnterDungeonButton, maxValue <= userStat);
		}
	}

	private void SetResetButton()
	{
		UIHelper.SetGrayscaleSprite(ResetButton, CheckDungeonCleared());
	}

	private bool CheckDungeonCleared()
	{
		TreasureHouseInfoData data = GameRoot.Instance.TreasureHouseInfoDataContainer.GetData(_playerTreasureHouseDungeonData.InfoIndex);
		if (data == null)
		{
			VespaDebug.LogError("treasureHouseInfoData (TableData) is null");
			return false;
		}
		int enterCount = data.EnterCount;
		int clearCount = _playerTreasureHouseInfo.ClearCount;
		return enterCount <= clearCount;
	}

	public void PlayChangeBoss()
	{
		_selectBossComponent.SelectToggle(isSelect: false);
		UIHelper.EnableGameObject(RewardWidget, enable: false);
		SetCheckSelectBossLabelEnable(isEnable: false);
		foreach (KeyValuePair<int, CreatureWidgetComponent> item in _creatureWidgetComponentDic)
		{
			CreatureWidgetComponent value = item.Value;
			if (!(value == null))
			{
				value.UICreatureWidget.SetAlpha(1f);
			}
		}
		UIHelper.RepositionGrid(SelectBossGrid);
		UIHelper.RepositionGrid(CreatureWidgetGrid);
		AddTouchDisabler("playChangeBoss");
	}

	public void EndChangeBoss()
	{
		RemoveTouchDisabler("PlayChangeBoss");
		if (SelectEffectWidgets != null)
		{
			UIGame3DWidget[] selectEffectWidgets = SelectEffectWidgets;
			foreach (UIGame3DWidget uIGame3DWidget in selectEffectWidgets)
			{
				if (uIGame3DWidget != null)
				{
					uIGame3DWidget.Play(null);
				}
			}
		}
		if (InitTreasureHouseData())
		{
			InitTreasureHouse();
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _selectBossComponent);
		FactoryBase.ReleaseAndClearList(_treasureHouseBossComponentList);
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
		FactoryBase.ReleaseAndClearList(_currencyIconComponentList);
		_currentCreatureWidget = null;
		_playerTreasureHouseDungeonData = null;
		_playerTreasureHouseInfo = null;
	}

	public override void OnClosing()
	{
		base.OnClosing();
		if (_selectBossComponent != null)
		{
			_selectBossComponent.SelectToggle(isSelect: false);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_treasureHouseBossComponentList); i++)
		{
			if (_treasureHouseBossComponentList[i] != null)
			{
				_treasureHouseBossComponentList[i].SelectToggle(isSelect: false);
			}
		}
		RelaseCreatureWidgetComp();
	}

	public void OnShowDesc()
	{
		UIHelper.EnableGameObject(DescTooltipWidget, enable: true);
	}

	public void OnHideDesc()
	{
		UIHelper.EnableGameObject(DescTooltipWidget, enable: false);
	}

	public void OnClickChangeBoss()
	{
		if (_listener == null)
		{
			return;
		}
		if (CheckDungeonCleared())
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TREASUREHOUSE_CHANGE_UNABLE"));
			return;
		}
		TreasureHouseInfoData data = GameRoot.Instance.TreasureHouseInfoDataContainer.GetData(_playerTreasureHouseDungeonData.InfoIndex);
		if (data == null)
		{
			VespaDebug.LogError("treasureHouseInfoData (TableData) is null");
			return;
		}
		string priceTypeStr = data.ChangeCurrencyType;
		PricedMessagePopup.EPriceType priceType = Vespa_String_Extension_Method.TOENUM(priceTypeStr, PricedMessagePopup.EPriceType.Gem);
		int price = data.ChangeCurrencyCost;
		string stringDirect = LocalizationManager.GetStringDirect("TREASUREHOUSE_BOSSCHANGE_POPUP_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("TREASUREHOUSE_BOSSCHANGE_POPUP_DESC");
		string stringDirect3 = LocalizationManager.GetStringDirect("TREASUREHOUSE_BOSSCHANGE_POPUP_CONFIRM");
		PricedMessagePopup.Open(stringDirect, stringDirect2, stringDirect3, priceType, price, delegate
		{
			CurrencyType currencyType = Vespa_String_Extension_Method.TOENUM(priceTypeStr, CurrencyType.Gem);
			_listener.OnClickChangeBoss(currencyType, price);
		});
	}

	public void OnClickProbability()
	{
		if (_playerTreasureHouseInfo != null)
		{
			TreasureHouseProbabilityPopup.Open(_playerTreasureHouseInfo.Index);
		}
	}

	public void OnClickEnterDungeon()
	{
		if (_listener != null)
		{
			_listener.OnClickEnterDungeon(_playerTreasureHouseDungeonData.ChapterIndex, _playerTreasureHouseDungeonData.DungeonIndex);
		}
	}

	void TreasureHouseBossComponent.IListener.OnClickTab(TreasureHouseBossComponent caller)
	{
		TreasureHouseDungeonData treasureHouseData = caller._treasureHouseData;
		if (treasureHouseData == null)
		{
			VespaDebug.LogError("treasureHouseData is null");
		}
		else
		{
			SetBossInfo(treasureHouseData);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>.OnChanged(NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo changedInfo)
	{
		if (changedInfo.currencyType != CurrencyType.TreasureHousePoint || _listener == null)
		{
			return;
		}
		_listener.RefreshTreasureHouseInfo(delegate
		{
			if (InitTreasureHouseData())
			{
				InitTreasureHouse();
			}
		});
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType == UserManager.UserStatType.TreasureHousePoint && InitTreasureHouseData())
		{
			InitTreasureHouse();
		}
	}
}
