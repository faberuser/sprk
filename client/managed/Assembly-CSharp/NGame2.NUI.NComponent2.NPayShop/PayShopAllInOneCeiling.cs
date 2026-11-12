using System.Collections;
using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopAllInOneCeiling : MonoBehaviour
{
	public interface IListener
	{
	}

	public GameObject Root;

	public UILabel ProgressLabel;

	public GameObject[] ProgressIndicateIcons;

	public UISprite[] SpriteProgressBars;

	public Transform[] RewardItemIconRoots;

	private IListener _listener;

	private int[] rewardIndices;

	private List<PayShopAllInOneRewardItemComponent> rewardItemIconComponentList = new List<PayShopAllInOneRewardItemComponent>();

	public void Setup(NewEquipGachaData data, int gachaCount, IListener listener)
	{
		rewardIndices = data.RewardIndices;
		if (data.Index != 11 || __Extension_Method_ICollection__.COUNT(rewardIndices) != 2)
		{
			Close();
			return;
		}
		InitRewardItemIconComponent(rewardIndices);
		RefreshRewardProgress(gachaCount, data.MileageAmountForReward * __Extension_Method_ICollection__.COUNT(rewardIndices));
	}

	private void InitRewardItemIconComponent(int[] rewardDataIndices)
	{
		if (rewardItemIconComponentList.Count > 0)
		{
			FactoryBase.ReleaseAndClearList(rewardItemIconComponentList);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RewardItemIconRoots) && !(RewardItemIconRoots[i] == null); i++)
		{
			PayShopAllInOneRewardItemComponent payShopAllInOneRewardItemComponent = PayShopAllInOneRewardItemComponent.PrefabAuto.Factory.Create(RewardItemIconRoots[i]);
			RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardDataIndices[i]);
			if (data == null)
			{
				break;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.Item1Code);
			if (dataByCode == null)
			{
				break;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(payShopAllInOneRewardItemComponent.transform);
			payShopAllInOneRewardItemComponent.InitRewardItemIcon(dataByCode);
			string stringDirect = LocalizationManager.GetStringDirect((i == 0) ? "ALLINONE_GACHA_CONFIRM_REWARD_10" : "ALLINONE_GACHA_CONFIRM_REWARD_20");
			UIHelper.SetLabelText(payShopAllInOneRewardItemComponent.rewardLabel, stringDirect);
			rewardItemIconComponentList.Add(payShopAllInOneRewardItemComponent);
		}
	}

	public void RefreshRewardProgress(int current, int max)
	{
		if (rewardIndices != null)
		{
			SetProgressLabel(current, max);
			SetProgressGaugeSprite(current, max);
			RefreshRewardIconState(current, max);
		}
	}

	private void SetProgressLabel(int gachaCount, float maxCount)
	{
		UIHelper.SetLabelText(ProgressLabel, gachaCount + " / " + maxCount);
	}

	private void SetProgressGaugeSprite(int gachaCount, float maxCount)
	{
		float num = Mathf.Min(gachaCount, maxCount);
		float num2 = ((gachaCount <= 10) ? (num / 10f) : 1f);
		float num3 = ((gachaCount <= 10) ? 0f : ((num - 10f) / 10f));
		UIHelper.SetSpriteFillAmount(SpriteProgressBars[0], num2);
		UIHelper.SetSpriteFillAmount(SpriteProgressBars[1], num3);
		UIHelper.EnableGameObject(ProgressIndicateIcons[0], num2 == 1f);
		UIHelper.EnableGameObject(ProgressIndicateIcons[1], num3 == 1f);
	}

	private void RefreshRewardIconState(int current, int max)
	{
		if (rewardItemIconComponentList.Count == 2)
		{
			float num = (float)Mathf.Min(current, max) / (float)max;
			if ((double)num < 0.5)
			{
				rewardItemIconComponentList[0].Refresh(ECompState.Current);
				rewardItemIconComponentList[1].Refresh(ECompState.NotYet);
			}
			else
			{
				rewardItemIconComponentList[0].Refresh(ECompState.Get);
				rewardItemIconComponentList[1].Refresh((!(num < 1f)) ? ECompState.Get : ECompState.Current);
			}
		}
	}

	public IEnumerator ResetRewardItemComponents()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemIconComponentList); i++)
		{
			yield return rewardItemIconComponentList[i].ResetItemComponent();
			yield return new WaitForSeconds(0.2f);
		}
	}

	public void Close()
	{
		FactoryBase.ReleaseAndClearList(rewardItemIconComponentList);
	}

	public void ShowUi(bool show)
	{
		UIHelper.EnableGameObject(Root, show);
	}
}
