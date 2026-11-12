using System;
using System.Collections.Generic;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class TreasureSlotSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private TreasureSlotSubComponent _this;

		private CachedHeroInfo _cachedHeroInfo;

		public override bool IsActive()
		{
			if (Owner == null)
			{
				return false;
			}
			if (Owner.EquipPartType != EquipPartType.Treasure)
			{
				return false;
			}
			if (_cachedHeroInfo == null)
			{
				return false;
			}
			if (IsSubSlotTreasure())
			{
				return false;
			}
			return true;
		}

		private bool IsSubSlotTreasure()
		{
			if (Owner.EquipItemInfo == null)
			{
				return false;
			}
			int equipSlotIndex = Owner.EquipPartType.GetEquipSlotIndex();
			int[] equipSlotIndices = Owner.EquipPartType.GetEquipSlotIndices();
			foreach (int num in equipSlotIndices)
			{
				EquipItemInfo equipItemInfoByPartIndex = _cachedHeroInfo.GetEquipItemInfoByPartIndex(num);
				if (equipItemInfoByPartIndex != null && equipItemInfoByPartIndex.ItemIndex == Owner.EquipItemInfo.ItemIndex && num != equipSlotIndex)
				{
					return true;
				}
			}
			return false;
		}

		public override void Release()
		{
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(CachedHeroInfo cachedHeroInfo)
		{
			_cachedHeroInfo = cachedHeroInfo;
			Refresh();
		}

		private void Refresh()
		{
			if (_this != null)
			{
				if (IsActive())
				{
					RefreshInfo();
				}
				else
				{
					ReleaseComponent();
				}
			}
		}

		public override void ChainUpdate()
		{
			if (IsActive() && Owner.gameObject.activeInHierarchy && _this == null)
			{
				CreateComponent();
			}
			if (_this != null)
			{
				ProxyBase.RearrangeDepth(Owner, _this.WidgetRoot);
			}
		}

		public override void RefreshInfo()
		{
			if (_this != null && Owner != null)
			{
				_this.Refresh(Owner.EquipPartType, _cachedHeroInfo);
			}
		}

		private void CreateComponent()
		{
			_this = PrefabAuto.Factory.Create(Owner.RootWidget.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_this.transform);
			RefreshInfo();
		}

		private void ReleaseComponent()
		{
			FactoryBase.SafeReleaseComponent(ref _this);
		}
	}

	[Serializable]
	public class SlotUiData
	{
		public UIWidget Root;

		public UIWidget EmptySlot;
	}

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/TreasureSlotSubComponent";

	public static PrefabAuto<TreasureSlotSubComponent> PrefabAuto = new PrefabAuto<TreasureSlotSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UIGrid TreasureGrid;

	public List<SlotUiData> TreasureSlotList = new List<SlotUiData>();

	public void Refresh(EquipPartType equipPartType, CachedHeroInfo cachedHeroInfo)
	{
		List<int> list = new List<int>(equipPartType.GetEquipSlotIndices());
		int equipSlotIndex = equipPartType.GetEquipSlotIndex();
		list.Remove(equipSlotIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(TreasureSlotList); i++)
		{
			if (__Extension_Method_ICollection__.COUNT(list) <= i)
			{
				UIHelper.EnableGameObject(TreasureSlotList[i].Root, enable: false);
				UIHelper.EnableGameObject(TreasureSlotList[i].EmptySlot, enable: true);
			}
			else
			{
				EquipItemInfo equipItemInfoByPartIndex = cachedHeroInfo.GetEquipItemInfoByPartIndex(list[i]);
				UIHelper.EnableGameObject(TreasureSlotList[i].Root, equipItemInfoByPartIndex != null);
				UIHelper.EnableGameObject(TreasureSlotList[i].EmptySlot, equipItemInfoByPartIndex == null);
			}
		}
		if (TreasureGrid != null)
		{
			TreasureGrid.Reposition();
		}
	}
}
