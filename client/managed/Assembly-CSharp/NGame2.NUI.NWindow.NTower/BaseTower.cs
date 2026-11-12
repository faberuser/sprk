using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class BaseTower : LobbyFullWindow
{
	public UILabel RemainResetTimeLabel;

	public UIButton ResetButton;

	public Transform ResetCurrencyRoot;

	public GameObject MiniGuideButton;

	private CurrencyIconComponent _resetCurrencyComponent;

	public TowerData TowerData { get; private set; }

	public TowerInfo TowerInfo { get; private set; }

	public int CompletedFloor { get; private set; }

	public int MaxFloor { get; private set; }

	public int TowerIndex => TowerData.TowerIndex;

	public RewardType ResetCostType { get; private set; }

	public int ResetCost { get; private set; }

	public void Init(TowerData towerData, TowerInfo towerInfo)
	{
		TowerData = towerData;
		MaxFloor = GetMaxFloor(TowerData.TowerIndex);
		SetMiniGuideButton(WindowGuideIndex);
		InitTowerInfo(towerInfo);
	}

	public override void Reset()
	{
		base.Reset();
		ClearResetCurrency();
	}

	public virtual bool CanReset()
	{
		return true;
	}

	public virtual void OnResetTower(TowerInfo towerInfo)
	{
	}

	protected void InitTowerInfo(TowerInfo towerInfo)
	{
		TowerInfo = towerInfo;
		CompletedFloor = towerInfo?.CompletedFloor ?? 0;
		InitResetInfo();
	}

	private void SetMiniGuideButton(int miniGuideIndex)
	{
		UIHelper.EnableGameObject(MiniGuideButton, miniGuideIndex != 0);
	}

	public static int GetMaxFloor(int towerIndex)
	{
		GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		int result = 0;
		for (int i = 1; i < 1000; i++)
		{
			TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(towerIndex, i);
			if (data == null || !TowerManagement.IsOpenedFloor(data))
			{
				break;
			}
			result = i;
		}
		return result;
	}

	private void ClearResetCurrency()
	{
		FactoryBase.SafeReleaseComponent(ref _resetCurrencyComponent);
	}

	private void InitResetRemainTime()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("DOWDungeonResetHour");
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		_ = currentServerLocalTime.Hour;
		int num = (int)(NVespa.Math.CreateDateTime(currentServerLocalTime.Year, currentServerLocalTime.Month, currentServerLocalTime.Day, valueInt, 0, 0) - currentServerLocalTime).TotalSeconds;
		if (num < 0)
		{
			num += 86400;
		}
		int num2 = num / 3600;
		int num3 = (num - num2 * 3600) / 60;
		string text = LocalizationManager.GetString(Tower.TOWER_REMAIN_TIME, num2, num3);
		UIHelper.SetLabelText(RemainResetTimeLabel, text);
	}

	private void InitResetButton()
	{
		if (!CanReset())
		{
			UIHelper.EnableButton(ResetButton, enable: false);
		}
		else if (((TowerInfo != null) ? TowerInfo.ResetCount : 0) >= TowerData.MaxResetCount || __Extension_Method_ICollection__.COUNT(TowerData.ResetCost) == 0)
		{
			UIHelper.EnableButton(ResetButton, enable: false);
		}
		else
		{
			UIHelper.EnableButton(ResetButton, enable: true);
		}
	}

	private void InitResetCurrency()
	{
		ClearResetCurrency();
		if (__Extension_Method_ICollection__.COUNT(TowerData.ResetCost) != 0)
		{
			int b = ((TowerInfo != null) ? TowerInfo.ResetCount : 0);
			int num = Mathf.Min(__Extension_Method_ICollection__.COUNT(TowerData.ResetCost) - 1, b);
			ResetCost = TowerData.ResetCost[num];
			ResetCostType = TowerData.ResetCostType;
			if (ResetCostType == RewardType.Gold)
			{
				CurrencyIconComponent currencyIconComponent = CurrencyIconComponent.PrefabAuto2.Factory.Create(ResetCurrencyRoot);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(currencyIconComponent.transform);
				int resetCost = ResetCost;
				CurrencyIconComponent.Listener listener = null;
				int num2 = resetCost;
				currencyIconComponent.Open(CurrencyType.Gold, num2, listener);
				_resetCurrencyComponent = currencyIconComponent;
			}
			else if (ResetCostType == RewardType.Gem)
			{
				CurrencyIconComponent currencyIconComponent2 = CurrencyIconComponent.PrefabAuto2.Factory.Create(ResetCurrencyRoot);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(currencyIconComponent2.transform);
				int resetCost2 = ResetCost;
				CurrencyIconComponent.Listener listener2 = null;
				int num3 = resetCost2;
				currencyIconComponent2.Open(CurrencyType.Gem, num3, listener2);
				_resetCurrencyComponent = currencyIconComponent2;
			}
		}
	}

	private void InitResetInfo()
	{
		InitResetRemainTime();
		InitResetButton();
		InitResetCurrency();
	}

	public void OnClickButton_MiniGuideMenu()
	{
		if (SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().CurrentMiniGuideData != null)
		{
			int[] gameGuideIndex = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().CurrentMiniGuideData.GameGuideIndex;
			SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<MiniGuidePopup>().Open(gameGuideIndex);
		}
	}
}
