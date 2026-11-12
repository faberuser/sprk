using System;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class NPCFriendshipItemComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void OnClickComponent(NPCFriendshipItemComponent component);
	}

	public static string PrefabPath = "Prefab2/Component/NPCRecruit/NPCFriendshipItemComponent";

	public static string PrefabPath2 = "Prefab2/Window/EventStepReward/EventStepPresentItemCompnent";

	public static PrefabAuto<NPCFriendshipItemComponent> PrefabAuto = new PrefabAuto<NPCFriendshipItemComponent>(PrefabPath);

	public static PrefabAuto<NPCFriendshipItemComponent> PrefabAuto2 = new PrefabAuto<NPCFriendshipItemComponent>(PrefabPath2);

	public UILabel ItemNameLabel;

	public UILabel AddFriendshipPointLabel;

	public Transform ItemIconRoot;

	public UISprite SelectedSprtie;

	public UILabel RestrictionLabel;

	private ItemIconComponent ItemIcon;

	private NPCFriendlyPointItemData PointItemdata;

	private NPCFriendlyInfo NPCFriendlyInfo;

	private Listener _Listener;

	public int FriendshipPoint
	{
		get
		{
			if (PointItemdata == null)
			{
				return 0;
			}
			return PointItemdata.FriendlyPoint;
		}
	}

	public int ItemMaxCount { get; private set; }

	public int ItemIndex { get; private set; }

	public bool CanGiftItem { get; private set; }

	private NpcFriendlyType FriendlyType { get; set; }

	public void Open(NPCFriendlyPointItemData pointItemData, NPCFriendlyInfo npcInfo, int maxCount, NpcFriendlyType friendlyType, Listener listener)
	{
		FriendlyType = friendlyType;
		PointItemdata = pointItemData;
		NPCFriendlyInfo = npcInfo;
		_Listener = listener;
		CanGiftItem = true;
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(pointItemData.ItemCode);
		if (dataByCode == null)
		{
			VespaDebug.LogError("NPCFriendlyPointItemData item is null");
			return;
		}
		ItemIndex = dataByCode.Index;
		ItemIcon = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconRoot);
		ItemIcon.Open(dataByCode);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(ItemIcon.transform);
		UpdateCount(maxCount);
		UpdateRestrictionState();
		string itemName = ItemNameBuilder.GetItemName(dataByCode);
		UIHelper.SetLabelText(ItemNameLabel, itemName);
		string text = "";
		UIHelper.SetLabelText(text: (FriendlyType == NpcFriendlyType.Recruit) ? LocalizationManager.GetString(NPCRecruit.NPC_ADDED_FRIENDLY_POINT, pointItemData.FriendlyPoint) : ((FriendlyType != NpcFriendlyType.EventChristmas) ? LocalizationManager.GetString(NPCRecruit.NPC_ADDED_CONTRIBUTION_POINT, pointItemData.FriendlyPoint, pointItemData.EventGiftPoint) : LocalizationManager.GetString(EVENTCHRISTMAS.ADD_REWARD_ITEM_DESC, pointItemData.FriendlyPoint)), label: AddFriendshipPointLabel);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref ItemIcon);
		ItemMaxCount = 0;
		Select(isSelect: false);
	}

	public void OnClickComponent()
	{
		if (_Listener != null && CanGiftItem)
		{
			_Listener.OnClickComponent(this);
		}
	}

	public void Select(bool isSelect)
	{
		UIHelper.EnableGameObject(SelectedSprtie, isSelect);
	}

	public void UpdateCount(int itemCount)
	{
		if (!(ItemIcon == null))
		{
			ItemMaxCount = itemCount;
			ItemIcon.SetItemCount(itemCount);
		}
	}

	public void UpdateRestrictionState()
	{
		ItemData itemData = GameRoot.Instance.ItemDataContainer.GetDataByCode(PointItemdata.ItemCode);
		NPCFriendlyPointData data = GameRoot.Instance.NPCFriendlyPointDataContainer.GetData(NPCFriendlyInfo.HeroIndex, NPCFriendlyInfo.Step);
		if (data != null && data.RestrictionItemCode != null)
		{
			if (!string.IsNullOrEmpty(Array.Find(data.RestrictionItemCode, (string val) => val == itemData.Code)))
			{
				CanGiftItem = false;
				string stringDirect = LocalizationManager.GetStringDirect(data.RestrictionLocalKey);
				UIHelper.SetLabelText(RestrictionLabel, stringDirect);
			}
			else
			{
				CanGiftItem = true;
			}
		}
		UIHelper.EnableGameObject(RestrictionLabel, !CanGiftItem);
	}

	public void OnShowTooltip()
	{
		if (!(ItemIcon == null))
		{
			ItemIcon.OnShowTooltip();
		}
	}

	public void OnHideTooltip()
	{
		if (!(ItemIcon == null))
		{
			ItemIcon.OnHideTooltip();
		}
	}
}
