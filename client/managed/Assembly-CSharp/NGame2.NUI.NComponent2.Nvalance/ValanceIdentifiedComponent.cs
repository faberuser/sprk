using NGame2.NUI.NComponent2.NStat;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.Nvalance;

public class ValanceIdentifiedComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Valance/ValanceIdentifiedComponent";

	public static PrefabAuto<ValanceIdentifiedComponent> PrefabAuto = new PrefabAuto<ValanceIdentifiedComponent>("Prefab2/Component/Valance/ValanceIdentifiedComponent");

	public Transform IconRoot;

	public UILabel LabelItemName;

	public UIScrollView ScrollView;

	public UIPanel ScrollViewPanel;

	public GameObject GoDrag;

	private StatListComponent _statListComponent;

	private ItemIconComponent _iconComponent;

	public int SlotIndex;

	private ItemData _cachedItemData;

	private EquipItemData _cachedEquipItemData;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(EquipItemInfo equipItemInfo)
	{
		if (IsValid(equipItemInfo))
		{
			SetItemIcon(equipItemInfo);
			SetName(equipItemInfo);
			SetState(equipItemInfo);
		}
	}

	public bool IsValid(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return false;
		}
		SlotIndex = equipItemInfo.SlotIndex;
		_cachedEquipItemData = equipItemInfo.EquipItemData;
		if (_cachedEquipItemData == null)
		{
			return false;
		}
		int itemIndex = equipItemInfo.ItemIndex;
		_cachedItemData = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemIndex);
		return _cachedItemData != null;
	}

	public void SetItemIcon(EquipItemInfo equipItemInfo)
	{
		if (!(IconRoot == null))
		{
			_iconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconRoot);
			if (_iconComponent != null)
			{
				_iconComponent.Open(equipItemInfo, null, equipItemInfo.EquippedHeroInfo);
				_iconComponent.ResetPosition();
			}
		}
	}

	public void SetName(EquipItemInfo equipItemInfo)
	{
		UIHelper.SetLabelText(LabelItemName, ItemNameBuilder.GetItemName(_cachedItemData, equipItemInfo));
	}

	public void SetState(EquipItemInfo equipItemInfo)
	{
		if (ScrollView == null || ScrollViewPanel == null)
		{
			return;
		}
		_statListComponent = StatListComponent.Create(ScrollViewPanel, 0, isValance: true);
		StatListComponent.AddOptionStat(_statListComponent, _cachedEquipItemData, equipItemInfo);
		StatListComponent.AddExtraOptionStat(_statListComponent, equipItemInfo);
		if (_statListComponent.RootTable.onReposition != null)
		{
			return;
		}
		_statListComponent.RootTable.onReposition = delegate
		{
			int totalHeight = _statListComponent.GetTotalHeight();
			float num = ScrollViewPanel.height;
			if (ScrollViewPanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				num -= ScrollViewPanel.clipSoftness.y * 2f;
			}
			bool enable = num <= (float)totalHeight;
			UIHelper.EnableGameObject(GoDrag, enable);
		};
	}

	public void Close()
	{
		if (_iconComponent != null)
		{
			FactoryBase.SafeReleaseComponent(ref _iconComponent);
		}
		if (_statListComponent != null)
		{
			FactoryBase.SafeReleaseComponent(ref _statListComponent);
		}
		SlotIndex = 0;
		_cachedEquipItemData = null;
		_cachedItemData = null;
	}
}
