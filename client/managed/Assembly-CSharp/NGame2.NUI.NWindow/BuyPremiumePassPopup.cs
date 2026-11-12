using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BuyPremiumePassPopup : BaseWindow
{
	public interface IListener
	{
		void BuyPremiumePass(int productIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/BuyPremiumePassPopup";

	public UIScrollGrid RewardGrid;

	private IListener _iListener;

	private List<Component> _rewardComponentList = new List<Component>();

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
		SetRewardList();
	}

	private void SetRewardList()
	{
		FactoryBase.ReleaseAndClearList(_rewardComponentList);
		if (_rewardComponentList == null || !GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager == null)
		{
			return;
		}
		PlayerKingsPassExp playerKingsPassExp = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.PlayerKingsPassExp;
		if (playerKingsPassExp == null)
		{
			return;
		}
		KingsPassSeasonMissionData kingsPassSeasonMissionData = null;
		for (int i = 0; i <= playerKingsPassExp.Level; i++)
		{
			kingsPassSeasonMissionData = GameRoot.Instance.KingsPassSeasonMissionDataContainer.GetDataByRewardKey(playerKingsPassExp.SeasonIndex, KingsPassType.PremiumPass, i);
			if (kingsPassSeasonMissionData != null)
			{
				_rewardComponentList.AddRange(UIHelper.CreateRewardItemIconComponentList(kingsPassSeasonMissionData.RewardIndex, RewardGrid, includeCurrency: true));
			}
		}
		UIHelper.RepositionGrid(RewardGrid);
	}

	public void OnClickBuy()
	{
		if (_iListener == null || !GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager == null)
		{
			return;
		}
		KingsPassSeaonInfo kingsPassSeaonInfo = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.KingsPassSeaonInfo;
		if (kingsPassSeaonInfo != null)
		{
			KingsPassSeasonData data = GameRoot.Instance.KingsPassSeasonDataContainer.GetData(kingsPassSeaonInfo.SeasonIndex);
			if (data != null)
			{
				_iListener.BuyPremiumePass(data.ProductIndex);
			}
		}
	}
}
