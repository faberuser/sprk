using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NClearMission;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class ClearMissionListComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface IListener
	{
		void OnClickGetReward(int index, Action onReceived);
	}

	public const string PrefabPath = "Prefab2/Component/ClearMission/ClearMissionListComponent";

	public static PrefabAuto<ClearMissionListComponent> PrefabAuto = new PrefabAuto<ClearMissionListComponent>("Prefab2/Component/ClearMission/ClearMissionListComponent");

	public UILabel LabelRewardCondition;

	public UILabel LabelGetReward;

	public UIButton ButtonGetReward;

	public UISprite SpriteGetBg;

	public UISprite SpriteStamp;

	public UISprite SpriteLockIcon;

	public UISimpleGrid RewardGrid;

	private List<RandomBoxDropIconComponent> _listItemIconComponent = new List<RandomBoxDropIconComponent>();

	private IListener _iListener;

	public ClearMissionProductMissionData MissionData { get; private set; }

	public void Open(IListener listener, ClearMissionProductMissionData missionData, bool receivedReward, bool isPaidReward, bool isPurchaseProduct)
	{
		_iListener = listener;
		MissionData = missionData;
		SetConditionMessage(missionData, receivedReward);
		SetGetRewardButtonState(missionData, receivedReward, isPaidReward, isPurchaseProduct);
		SetRewardItem(missionData.RewardIndex);
	}

	public void RefreshGetRewardButtonState(bool receivedReward, bool isPaidReward, bool isPurchaseProduct)
	{
		SetGetRewardButtonState(MissionData, receivedReward, isPaidReward, isPurchaseProduct);
	}

	private void SetGetRewardButtonState(ClearMissionProductMissionData missionData, bool receivedReward, bool isPaidReward, bool isPurchaseProduct)
	{
		bool flag = SingletonUpdater<ClearMissionManager>.instance.CanReceiveRewardMission(missionData);
		bool flag2 = false;
		if (isPaidReward)
		{
			flag2 = isPurchaseProduct && flag && !receivedReward;
			UIHelper.EnableGameObject(SpriteLockIcon, !isPurchaseProduct);
		}
		else
		{
			flag2 = flag && !receivedReward;
		}
		SetGetRewardButton(flag2, receivedReward, isPaidReward, isPurchaseProduct);
		if (receivedReward)
		{
			SetEffectEnd();
		}
	}

	private void SetConditionMessage(ClearMissionProductMissionData missionData, bool receivedReward)
	{
		string missionDesc = missionData.MissionDesc;
		object[] missionDescStateValue = missionData.MissionDescStateValue;
		string text = LocalizationManager.GetStringDirect(missionDesc, missionDescStateValue);
		if (missionData.ReqProgress > 1)
		{
			long num = 0L;
			num = ((!receivedReward) ? (GlobalAutoBehaviour<AccountManager>.instance.ClearMissionManager.Get(missionData.Index)?.Progress ?? 0) : MissionData.ReqProgress);
			string stringDirect = LocalizationManager.GetStringDirect("CLEARMISSION_PASSPOPUP_PROGRESS", num, missionData.ReqProgress);
			text += stringDirect;
		}
		UIHelper.SetLabelText(LabelRewardCondition, text);
	}

	private void SetRewardItem(int rewardIndex)
	{
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data == null)
		{
			return;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		if (__Extension_Method_ICollection__.COUNT(rewardItemList) == 0)
		{
			return;
		}
		foreach (RewardElementData item in rewardItemList)
		{
			RandomBoxDropIconComponent randomBoxDropIconComponent = UIHelper.AddGridItem(RewardGrid, RandomBoxDropIconComponent.PrefabAuto);
			if (!(randomBoxDropIconComponent == null))
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
				if (dataByCode != null && dataByCode.Type == ItemType.Equip)
				{
					item.ItemStar = item.ItemStarMax;
				}
				randomBoxDropIconComponent.Open(item, disableFixed: true);
				_listItemIconComponent.Add(randomBoxDropIconComponent);
			}
		}
		UIHelper.RepositionGrid(RewardGrid);
	}

	private void SetGetRewardButton(bool enableButton, bool receivedReward, bool isPaidReward, bool isPurchaseProduct)
	{
		if (isPaidReward && !isPurchaseProduct)
		{
			UIHelper.SetLabelText(LabelGetReward, string.Empty);
		}
		else
		{
			string stringDirect = LocalizationManager.GetStringDirect(receivedReward ? "BENEFIT_NEWBIE_REWARDCOMPLETE" : "BENEFIT_NEWBIE_REWARDGET");
			UIHelper.SetLabelText(LabelGetReward, stringDirect);
		}
		UIHelper.EnableButton(ButtonGetReward, enableButton, grayscaleIfDisable: true);
		UIHelper.SetWidgetGrayScale(LabelGetReward, !enableButton);
	}

	private void ReceivedRewardComplete()
	{
		ClearMissionProductData dataByGroupIndex = GameRoot.Instance.ClearMissionProductDataContainer.GetDataByGroupIndex(MissionData.ProductGroupIndex);
		bool isPaidReward = dataByGroupIndex.PurchaseType != ClearMissionPurchaseType.Free;
		bool isPurchaseProduct = SingletonUpdater<ClearMissionManager>.instance.IsPurchasedProduct(dataByGroupIndex);
		if (GlobalAutoBehaviour<AccountManager>.instance.ClearMissionManager.IsReceivedReward(MissionData.Index))
		{
			SetGetRewardButton(enableButton: false, receivedReward: true, isPaidReward, isPurchaseProduct);
			PlayEffect();
		}
		else
		{
			SetGetRewardButton(enableButton: true, receivedReward: false, isPaidReward, isPurchaseProduct);
		}
	}

	private void PlayEffect()
	{
		UIHelper.EnableGameObject(SpriteGetBg, enable: true);
		TweenScale component = SpriteStamp.GetComponent<TweenScale>();
		if (component != null)
		{
			component.enabled = true;
			component.ResetToBeginning();
			component.PlayForward();
		}
		TweenAlpha component2 = SpriteStamp.GetComponent<TweenAlpha>();
		if (component2 != null)
		{
			component2.enabled = true;
			component2.ResetToBeginning();
			component2.PlayForward();
		}
	}

	private void SetEffectEnd()
	{
		UIHelper.EnableGameObject(SpriteGetBg, enable: true);
		TweenScale component = SpriteStamp.GetComponent<TweenScale>();
		if (component != null)
		{
			component.Sample(1f, isFinished: true);
		}
		TweenAlpha component2 = SpriteStamp.GetComponent<TweenAlpha>();
		if (component2 != null)
		{
			component2.Sample(1f, isFinished: true);
		}
	}

	public void OnClickGetReward()
	{
		if (_iListener != null)
		{
			UIHelper.EnableButton(ButtonGetReward, enable: false, grayscaleIfDisable: true);
			_iListener.OnClickGetReward(MissionData.Index, ReceivedRewardComplete);
		}
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		UIHelper.SetLabelText(LabelRewardCondition, string.Empty);
		Clear();
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Clear();
	}

	private void Clear()
	{
		FactoryBase.ReleaseAndClearList(_listItemIconComponent);
		UIHelper.EnableGameObject(SpriteGetBg, enable: false);
		UIHelper.EnableButton(ButtonGetReward, enable: true, grayscaleIfDisable: true);
		TweenScale component = SpriteStamp.GetComponent<TweenScale>();
		if (component != null)
		{
			component.enabled = false;
			component.ResetToBeginning();
		}
		TweenAlpha component2 = SpriteStamp.GetComponent<TweenAlpha>();
		if (component2 != null)
		{
			component2.enabled = false;
			component2.ResetToBeginning();
		}
	}
}
