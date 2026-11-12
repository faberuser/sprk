using System.Collections.Generic;
using NGame2.NUtil.NStringProxy;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class MultiplayRewardItemComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void OnClick(MultiplayRewardItemComponent component);
	}

	public class MemberIndexInfo
	{
		public int MemberIndex;

		public GameObject MemberIndexObject;
	}

	public enum BiddingState
	{
		None,
		Single,
		Competition
	}

	public const string PrefabPath = "Prefab2/Component/Battle/MultiplayRewardItemComponent";

	public static PrefabAuto<MultiplayRewardItemComponent> PrefabAuto = new PrefabAuto<MultiplayRewardItemComponent>("Prefab2/Component/Battle/MultiplayRewardItemComponent");

	public Transform IconParent;

	public UILabel ItemNameLabel;

	public UILabel ItemCountLabel;

	public UIGrid MemberIndexGrid;

	public GameObject MemberIndexTemplate;

	public UILabel OwnerNameLabel;

	public GameObject[] BiddingStates;

	public GameObject SelectedObject;

	public GameObject BonusImage;

	private Listener _listener;

	private List<MemberIndexInfo> _memberIndexObjectList = new List<MemberIndexInfo>();

	private ItemIconComponent _itemIconComponent;

	public int RewardNo { get; private set; }

	public RewardItemInfo RewardItemInfo { get; private set; }

	public void Open(Listener listener, int rewardNo, RewardItemInfo rewardItemInfo)
	{
		_listener = listener;
		RewardNo = rewardNo;
		RewardItemInfo = rewardItemInfo;
		InitItem();
		UpdateBiddingState();
		UIHelper.EnableGameObject(MemberIndexTemplate, enable: false);
		UIHelper.SetLabelText(OwnerNameLabel, "");
		SetSelected(selected: false);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClearObjectList();
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
	}

	private void ClearObjectList()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_memberIndexObjectList); i++)
		{
			VespaObject.DestroyGoAndDetachFromParent(_memberIndexObjectList[i].MemberIndexObject);
		}
		_memberIndexObjectList.Clear();
	}

	private void InitItem()
	{
		if (RewardItemInfo == null)
		{
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(RewardItemInfo.Code);
		if (dataByCode == null)
		{
			VespaDebug.LogError("ItemData not found : code=" + RewardItemInfo.Code);
			return;
		}
		UIHelper.SetLabelText(ItemNameLabel, ItemNameBuilder.GetItemName(dataByCode, null, isAppendIndex: false));
		UIHelper.SetLabelText(ItemCountLabel, (RewardItemInfo.Count > 1) ? RewardItemInfo.Count.ToString() : "");
		_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconParent);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		if (dataByCode.Type == ItemType.Equip)
		{
			EquipItemInfo equipItemInfo = new EquipItemInfo();
			equipItemInfo.ItemIndex = dataByCode.Index;
			equipItemInfo.Star = RewardItemInfo.Star;
			_itemIconComponent.Open(dataByCode, equipItemInfo, null);
		}
		else
		{
			_itemIconComponent.Open(dataByCode);
			ItemIconComponent itemIconComponent = _itemIconComponent;
			int count = RewardItemInfo.Count;
			Color? color = null;
			int num = count;
			itemIconComponent.SetItemCount(num, color);
		}
		UIHelper.EnableGameObject(BonusImage, RewardItemInfo.IsBonus);
	}

	public void UpdateBiddingState()
	{
		if (_memberIndexObjectList.Count >= 2)
		{
			SetBiddingState(BiddingState.Competition);
		}
		else if (_memberIndexObjectList.Count == 1)
		{
			SetBiddingState(BiddingState.Single);
		}
		else
		{
			SetBiddingState(BiddingState.None);
		}
	}

	public void SetBiddingState(BiddingState state)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BiddingStates); i++)
		{
			UIHelper.EnableGameObject(BiddingStates[i], i == (int)state);
		}
	}

	private void HideBiddingState()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BiddingStates); i++)
		{
			UIHelper.EnableGameObject(BiddingStates[i], enable: false);
		}
	}

	public void AddMemberIndex(int memberIndex)
	{
		if (!(MemberIndexTemplate == null) && !(MemberIndexGrid == null))
		{
			RemoveMemberIndex(memberIndex);
			GameObject gameObject = Object.Instantiate(MemberIndexTemplate);
			gameObject.SetActive(value: true);
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, MemberIndexGrid.transform);
			gameObject.GetComponent<UISpriteSelector>().SelectByIndex(memberIndex);
			gameObject.name = $"{memberIndex + 1}P";
			_memberIndexObjectList.Add(new MemberIndexInfo
			{
				MemberIndex = memberIndex,
				MemberIndexObject = gameObject
			});
			MemberIndexGrid.Reposition();
		}
	}

	public void RemoveMemberIndex(int memberIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_memberIndexObjectList); i++)
		{
			MemberIndexInfo memberIndexInfo = _memberIndexObjectList[i];
			if (memberIndexInfo.MemberIndex == memberIndex)
			{
				VespaObject.DestroyGoAndDetachFromParent(memberIndexInfo.MemberIndexObject, base.transform);
				_memberIndexObjectList.RemoveAt(i);
				MemberIndexGrid.Reposition();
				break;
			}
		}
	}

	private void ClearMemberIndexExceptOne()
	{
		ClearObjectList();
		MemberIndexGrid.Reposition();
	}

	public void SetOwner(string ownerName, int memberIndex)
	{
		ClearMemberIndexExceptOne();
		AddMemberIndex(memberIndex);
		HideBiddingState();
		UIHelper.SetLabelText(OwnerNameLabel, ownerName);
	}

	public void SetSelected(bool selected)
	{
		UIHelper.EnableGameObject(SelectedObject, selected);
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
