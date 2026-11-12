using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class KingsPassBuyLevelPopup : BaseWindow, CountInputField.IListener
{
	public interface IListener
	{
		void BuyLevel(int seasonIndex, int level);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/KingsPass/KingsPassBuyLevelPopup";

	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UIBaseGrid RewardGrid;

	public UIButton BuyButton;

	public CountInputField CountInputField;

	private Action<int> _onYes;

	private IListener _iListener;

	private KingsPassSeaonInfo _kingsPassSeasonInfo;

	private KingsPassSeasonData _kingsPassSeasonData;

	private List<Component> _rewardComponentList = new List<Component>();

	private bool CheckBuyPremiumePass
	{
		get
		{
			if (_kingsPassSeasonData == null)
			{
				return false;
			}
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return false;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager == null)
			{
				return false;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(_kingsPassSeasonData.ProductIndex) != null;
		}
	}

	private int MyLevel
	{
		get
		{
			int num = 0;
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return num;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager == null)
			{
				return num;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.PlayerKingsPassExp?.Level ?? num;
		}
	}

	private int UpLevel
	{
		get
		{
			int myLevel = MyLevel;
			if (CountInputField == null)
			{
				return myLevel;
			}
			long currentCount = CountInputField.CurrentCount;
			int num = myLevel;
			checked
			{
				long num2 = num + currentCount;
				return (int)num2;
			}
		}
	}

	public void Open(IListener iListener)
	{
		Open(delegate
		{
			Init(iListener);
		}, null);
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager == null)
		{
			return;
		}
		_kingsPassSeasonInfo = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.KingsPassSeaonInfo;
		if (_kingsPassSeasonInfo != null)
		{
			_kingsPassSeasonData = GameRoot.Instance.KingsPassSeasonDataContainer.GetData(_kingsPassSeasonInfo.SeasonIndex);
			if (_kingsPassSeasonData != null)
			{
				int currentLevel = _kingsPassSeasonInfo.CurrentLevel;
				int myLevel = MyLevel;
				int maxCount = currentLevel - myLevel;
				InitCountInputField(maxCount);
				SetRewardList();
				SetUpLevelPrice();
			}
		}
	}

	private void InitCountInputField(int maxCount)
	{
		if (!(CountInputField == null))
		{
			CountInputField countInputField = CountInputField;
			CountInputField.CountInputFieldInfo obj = new CountInputField.CountInputFieldInfo
			{
				InitCount = ((maxCount > 0) ? 1 : 0)
			};
			obj.MaxCount = maxCount;
			obj.InitPrice = 0L;
			countInputField.Open(this, obj);
		}
	}

	private void SetRewardList()
	{
		ReleaseRewardComponentList();
		UIHelper.SetLabelText(MessageLabel, "-");
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || CountInputField == null || _kingsPassSeasonInfo == null || _kingsPassSeasonData == null)
		{
			return;
		}
		int myLevel = MyLevel;
		int upLevel = UpLevel;
		for (int i = myLevel + 1; i <= UpLevel; i++)
		{
			KingsPassSeasonMissionData dataByRewardKey = GameRoot.Instance.KingsPassSeasonMissionDataContainer.GetDataByRewardKey(_kingsPassSeasonInfo.SeasonIndex, KingsPassType.KingsPass, i);
			if (dataByRewardKey == null)
			{
				continue;
			}
			RewardData data = GameRoot.Instance.RewardDataContainer.GetData(dataByRewardKey.RewardIndex);
			if (data != null)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.ItemCode1);
				if (dataByCode != null)
				{
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardGrid, ItemIconComponent.PrefabAuto);
					itemIconComponent.Open(dataByCode);
					int item1CountMax = data.Item1CountMax;
					Color? color = null;
					int num = item1CountMax;
					itemIconComponent.SetItemCount(num, color);
					_rewardComponentList.Add(itemIconComponent);
				}
			}
		}
		if (CheckBuyPremiumePass)
		{
			for (int j = myLevel + 1; j <= UpLevel; j++)
			{
				KingsPassSeasonMissionData dataByRewardKey2 = GameRoot.Instance.KingsPassSeasonMissionDataContainer.GetDataByRewardKey(_kingsPassSeasonInfo.SeasonIndex, KingsPassType.PremiumPass, j);
				if (dataByRewardKey2 == null)
				{
					continue;
				}
				RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(dataByRewardKey2.RewardIndex);
				if (data2 != null)
				{
					ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(data2.ItemCode1);
					if (dataByCode2 != null)
					{
						ItemIconComponent itemIconComponent2 = UIHelper.AddGridItem(RewardGrid, ItemIconComponent.PrefabAuto);
						itemIconComponent2.Open(dataByCode2);
						int item1CountMax2 = data2.Item1CountMax;
						Color? color2 = null;
						int num2 = item1CountMax2;
						itemIconComponent2.SetItemCount(num2, color2);
						_rewardComponentList.Add(itemIconComponent2);
					}
				}
			}
		}
		UIHelper.RepositionGrid(RewardGrid);
		SetRewardDesc(upLevel, _rewardComponentList.Count);
	}

	private void SetRewardDesc(int level, int rewardCount)
	{
		string stringDirect = LocalizationManager.GetStringDirect("KINGSPASS_LEVEL_UP_DESC", level, rewardCount);
		UIHelper.SetLabelText(MessageLabel, stringDirect);
	}

	private void SetUpLevelPrice()
	{
		if (_kingsPassSeasonData == null || CountInputField == null)
		{
			return;
		}
		int myLevel = MyLevel;
		int upLevel = UpLevel;
		int num = 0;
		KingsPassLevelData kingsPassLevelData = null;
		for (int i = myLevel + 1; i <= UpLevel; i++)
		{
			kingsPassLevelData = GameRoot.Instance.KingsPassLevelDataContainer.GetData(_kingsPassSeasonData.LevelIndex, upLevel);
			if (kingsPassLevelData != null)
			{
				num += kingsPassLevelData.LevelUpPrice;
			}
		}
		long gem = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gem;
		int num2 = num;
		bool num3 = gem >= num2;
		UIButton buyButton = BuyButton;
		int num4 = num;
		UIHelper.EnableButton(buyButton, gem >= num4);
		Color labelColor = (num3 ? Color.white : Color.red);
		CountInputField.SetPrice(num);
		CountInputField.SetLabelColor(labelColor);
	}

	public override void Reset()
	{
		base.Reset();
		ReleaseRewardComponentList();
	}

	private void ReleaseRewardComponentList()
	{
		FactoryBase.ReleaseAndClearList(_rewardComponentList);
	}

	public void OnClickYes()
	{
		if (_iListener != null)
		{
			_iListener.BuyLevel(_kingsPassSeasonInfo.SeasonIndex, UpLevel);
		}
	}

	public override void OnSystemBackButton()
	{
		Close();
	}

	void CountInputField.IListener.ChangeCount(bool isMax)
	{
		SetRewardList();
		SetUpLevelPrice();
	}
}
