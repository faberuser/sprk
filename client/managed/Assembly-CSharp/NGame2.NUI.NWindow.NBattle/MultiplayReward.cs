using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.GetMultiplayReward;
using NShared.NLocalization;
using NShared.NParty;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class MultiplayReward : BaseWindow, MultiplayRewardItemComponent.Listener
{
	public interface Listener
	{
		bool IsPartyValid();

		void OnAbandon(MultiplayReward caller);

		void OnBid(MultiplayReward caller);

		void OnRewardCalculationEnd(MultiplayReward caller, NShared.GetMultiplayReward.Response response);

		void RequestGetReward();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/MultiplayReward";

	public UIScrollGrid RewardItemGrid;

	public Transform ItemInfoRoot;

	public Transform ItemIconParent;

	public UILabel ItemNameLabel;

	public UILabel ItemTypeLabel;

	public UILabel ItemDescLabel;

	public UISimpleGrid BidStateGrid;

	public UIButton BidButton;

	public UIButton AbandonButton;

	public UIButton CloseButton;

	public UILabel RemainTimeLabel;

	private Listener _listener;

	private RewardItemInfo[] _rewardItemInfos;

	private MemberInfo[] _partyMemberInfos;

	private List<MultiplayRewardItemComponent> _rewardList = new List<MultiplayRewardItemComponent>();

	private List<MultiplayRewardBidStateComponent> _bidStateList = new List<MultiplayRewardBidStateComponent>();

	private ItemIconComponent _itemIconComponent;

	private bool _rewardSelected;

	private bool _allMemberRewardSelected;

	private float _timeoutTime;

	private NShared.GetMultiplayReward.Response _response;

	private long[] _rewardableAccountIds;

	[Header("- AnimationDelayTimeSetting")]
	[SerializeField]
	private float _rewardAnimDelayTime = 1f;

	[SerializeField]
	private float _rewardDiceAnimDelayTime = 2f;

	public RewardItemInfo CurrentRewardItemInfo { get; private set; }

	public int CurrentRewardItemNo { get; private set; }

	public void Open(Listener listener, float timeoutTime, RewardItemInfo[] rewardItemInfos, MemberInfo[] partyMemberInfos, EndCampaignProxy endCampaignProxy, bool canSelectReward)
	{
		Open(delegate
		{
			Init(listener, timeoutTime, rewardItemInfos, partyMemberInfos, endCampaignProxy, canSelectReward);
		}, null);
	}

	public void Init(Listener listener, float timeoutTime, RewardItemInfo[] rewardItemInfos, MemberInfo[] partyMemberInfos, EndCampaignProxy endCampaignProxy, bool canSelectReward)
	{
		_listener = listener;
		_rewardItemInfos = rewardItemInfos;
		_partyMemberInfos = partyMemberInfos;
		_timeoutTime = timeoutTime;
		_rewardableAccountIds = endCampaignProxy.MultiplayMemberIds;
		SetupRewardItems();
		SetupBidState();
		InitItemInfo(-1, null);
		ClickFirstItem();
		UIHelper.EnableGameObject(BidButton, canSelectReward);
		UIHelper.EnableGameObject(AbandonButton, canSelectReward);
		UIHelper.EnableGameObject(CloseButton, enable: false);
	}

	public override void Reset()
	{
		base.Reset();
		ClearItem();
		ClearBidState();
		ClearItemIcon();
		CurrentRewardItemInfo = null;
		CurrentRewardItemNo = -1;
		_rewardSelected = false;
		_allMemberRewardSelected = false;
		_timeoutTime = 0f;
		_response = null;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateRemainTime();
		UpdateRequestGetReward();
		UpdatePartyValidity();
	}

	private int GetRemainTime()
	{
		return Mathf.Max((int)(_timeoutTime - Time.time), 0);
	}

	private void UpdateRemainTime()
	{
		UIHelper.SetLabelText(RemainTimeLabel, GetRemainTime());
	}

	private void UpdateRequestGetReward()
	{
		if (!_allMemberRewardSelected && _timeoutTime > 0f && GetRemainTime() == 0)
		{
			_allMemberRewardSelected = true;
			if (_listener != null)
			{
				_listener.RequestGetReward();
			}
		}
	}

	private void UpdatePartyValidity()
	{
		if (_listener != null && !_listener.IsPartyValid())
		{
			CenterMessage.Open(LocalizationManager.GetString(Party.PARTY_DESTROYED));
			Close();
		}
	}

	public void OnPartyMemberSelectRewardItem(MemberInfo memberInfo)
	{
		foreach (MultiplayRewardBidStateComponent bidState in _bidStateList)
		{
			if (bidState.MemberInfo.AccountId == memberInfo.AccountId)
			{
				bidState.SetSelectedRewardItemNo(memberInfo.SelectedRewardItemNo);
				break;
			}
		}
		foreach (MultiplayRewardItemComponent reward in _rewardList)
		{
			if (reward.RewardNo == memberInfo.SelectedRewardItemNo)
			{
				reward.AddMemberIndex(memberInfo.MemberIndex);
			}
			else
			{
				reward.RemoveMemberIndex(memberInfo.MemberIndex);
			}
			reward.UpdateBiddingState();
		}
	}

	private void ClearItem()
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
	}

	private void ClearBidState()
	{
		FactoryBase.ReleaseAndClearList(_bidStateList);
	}

	private void SetupRewardItems()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rewardItemInfos); i++)
		{
			RewardItemInfo rewardItemInfo = _rewardItemInfos[i];
			MultiplayRewardItemComponent multiplayRewardItemComponent = UIHelper.AddGridItem(RewardItemGrid, MultiplayRewardItemComponent.PrefabAuto);
			if (!(multiplayRewardItemComponent == null))
			{
				multiplayRewardItemComponent.Open(this, i, rewardItemInfo);
				_rewardList.Add(multiplayRewardItemComponent);
			}
		}
		MemberInfo[] partyMemberInfos = _partyMemberInfos;
		foreach (MemberInfo memberInfo in partyMemberInfos)
		{
			if (memberInfo.DidSelectReward())
			{
				OnPartyMemberSelectRewardItem(memberInfo);
			}
		}
		UIHelper.RepositionGrid(RewardItemGrid);
	}

	private MultiplayRewardItemComponent GetRewardItemComponent(int rewardItemNo)
	{
		if (rewardItemNo < __Extension_Method_ICollection__.COUNT(_rewardList))
		{
			return _rewardList[rewardItemNo];
		}
		return null;
	}

	private void SetupBidState()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_partyMemberInfos); i++)
		{
			MemberInfo memberInfo = _partyMemberInfos[i];
			if (_rewardableAccountIds == null || TypeIterator.Contains(_rewardableAccountIds, memberInfo.AccountId))
			{
				MultiplayRewardBidStateComponent multiplayRewardBidStateComponent = UIHelper.AddGridItem(BidStateGrid, MultiplayRewardBidStateComponent.PrefabAuto);
				if (!(multiplayRewardBidStateComponent == null))
				{
					multiplayRewardBidStateComponent.Open(memberInfo);
					_bidStateList.Add(multiplayRewardBidStateComponent);
				}
			}
		}
	}

	private MultiplayRewardBidStateComponent GetBidStateComponent(long accountId)
	{
		foreach (MultiplayRewardBidStateComponent bidState in _bidStateList)
		{
			if (bidState.MemberInfo != null && bidState.MemberInfo.AccountId == accountId)
			{
				return bidState;
			}
		}
		return null;
	}

	void MultiplayRewardItemComponent.Listener.OnClick(MultiplayRewardItemComponent component)
	{
		OnClickRewardItem(component);
	}

	private void OnClickRewardItem(MultiplayRewardItemComponent component)
	{
		InitItemInfo(component.RewardNo, component.RewardItemInfo);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rewardList); i++)
		{
			_rewardList[i].SetSelected(_rewardList[i] == component);
		}
	}

	private void InitItemInfo(int rewardNo, RewardItemInfo rewardItemInfo)
	{
		CurrentRewardItemInfo = rewardItemInfo;
		CurrentRewardItemNo = rewardNo;
		if (CurrentRewardItemInfo == null)
		{
			UIHelper.EnableGameObject(ItemInfoRoot, enable: false);
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardItemInfo.Code);
		if (dataByCode == null)
		{
			UIHelper.EnableGameObject(ItemInfoRoot, enable: false);
			return;
		}
		UIHelper.EnableGameObject(ItemInfoRoot, enable: true);
		UIHelper.SetLabelText(ItemNameLabel, ItemNameBuilder.GetItemName(dataByCode));
		UIHelper.SetLabelText(ItemDescLabel, dataByCode.Description);
		UIHelper.SetLabelText(ItemTypeLabel, dataByCode.Type);
		SetupIcon(rewardItemInfo, dataByCode);
	}

	private void ClickFirstItem()
	{
		if (__Extension_Method_ICollection__.COUNT(_rewardList) > 0)
		{
			OnClickRewardItem(_rewardList[0]);
		}
	}

	private void ClearItemIcon()
	{
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
	}

	private void SetupIcon(RewardItemInfo rewardItemInfo, ItemData itemData)
	{
		ClearItemIcon();
		_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconParent);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		if (itemData.Type == ItemType.Equip)
		{
			EquipItemInfo equipItemInfo = new EquipItemInfo();
			equipItemInfo.ItemIndex = itemData.Index;
			equipItemInfo.Star = rewardItemInfo.Star;
			_itemIconComponent.Open(itemData, equipItemInfo, null);
		}
		else
		{
			_itemIconComponent.Open(itemData);
			ItemIconComponent itemIconComponent = _itemIconComponent;
			int count = rewardItemInfo.Count;
			Color? color = null;
			int num = count;
			itemIconComponent.SetItemCount(num, color);
		}
	}

	public void OnClickAbandon()
	{
		if (!_rewardSelected && !_allMemberRewardSelected && _listener != null)
		{
			_listener.OnAbandon(this);
		}
	}

	public void OnClickBid()
	{
		if (!_rewardSelected && !_allMemberRewardSelected && _listener != null)
		{
			_listener.OnBid(this);
		}
	}

	public void OnClickClose()
	{
		if (_listener != null)
		{
			_listener.OnRewardCalculationEnd(this, _response);
		}
	}

	public void SetSelected()
	{
		UIHelper.EnableButton(BidButton, enable: false);
		UIHelper.EnableButton(AbandonButton, enable: false);
		_rewardSelected = true;
	}

	public void StartRewardCalculation(NShared.GetMultiplayReward.Response response)
	{
		SetSelected();
		_timeoutTime = 0f;
		_allMemberRewardSelected = true;
		_response = response;
		VespaCoroutine.Start(coRewardCaculationAnim(response), null, null);
	}

	private IEnumerator coRewardCaculationAnim(NShared.GetMultiplayReward.Response response)
	{
		UIHelper.EnableGameObject(BidButton, enable: false);
		UIHelper.EnableGameObject(AbandonButton, enable: false);
		yield return new WaitForSeconds(_rewardAnimDelayTime);
		for (int rewardItemNo = 0; rewardItemNo < __Extension_Method_ICollection__.COUNT(response.RewardWinners); rewardItemNo++)
		{
			RewardWinner rewardWinner = response.RewardWinners[rewardItemNo];
			long[] applicantIds = rewardWinner.ApplicantIds;
			int[] diceNumbers = rewardWinner.DiceNumbers;
			long winnerId = rewardWinner.WinnerId;
			foreach (MultiplayRewardBidStateComponent bidState in _bidStateList)
			{
				bidState.ClearState();
			}
			if (__Extension_Method_ICollection__.COUNT(applicantIds) >= 2)
			{
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(applicantIds); i++)
				{
					long accountId = applicantIds[i];
					int diceNumber = ((i >= __Extension_Method_ICollection__.COUNT(diceNumbers)) ? 1 : diceNumbers[i]);
					MultiplayRewardBidStateComponent bidStateComponent = GetBidStateComponent(accountId);
					if ((bool)bidStateComponent)
					{
						bidStateComponent.ShowDiceNumber(diceNumber);
					}
				}
				yield return new WaitForSeconds(_rewardDiceAnimDelayTime);
			}
			MultiplayRewardItemComponent rewardItemComponent = GetRewardItemComponent(rewardItemNo);
			if ((bool)rewardItemComponent)
			{
				MemberInfo memberInfo = _partyMemberInfos.FirstOrDefault((MemberInfo e) => e.AccountId == winnerId);
				if (memberInfo != null)
				{
					rewardItemComponent.SetOwner(memberInfo.Nick, memberInfo.MemberIndex);
				}
			}
			yield return new WaitForSeconds(_rewardAnimDelayTime);
		}
		UIHelper.EnableGameObject(CloseButton, enable: true);
	}

	public void EnableGetMultiRewardButton()
	{
		UIHelper.EnableGameObject(BidButton, enable: false);
		UIHelper.EnableGameObject(AbandonButton, enable: false);
		UIHelper.EnableGameObject(CloseButton, enable: true);
	}
}
