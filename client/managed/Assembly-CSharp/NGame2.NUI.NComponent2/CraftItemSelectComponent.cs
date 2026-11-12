using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class CraftItemSelectComponent : UIScrollItemBehaviour, UICamera.IOnClickListener, ItemIconComponent.Listener
{
	public interface Listener
	{
		void OnClick(int craftIndex);

		void OnItemDescClick(int craftIndex);
	}

	public const string PrefabPath = "Prefab2/Component/Inventory/CraftItemSelectComponent";

	public static PrefabAuto<CraftItemSelectComponent> PrefabAuto = new PrefabAuto<CraftItemSelectComponent>("Prefab2/Component/Inventory/CraftItemSelectComponent");

	public UISimpleGrid ItemGrid;

	private ItemIconComponent ItemIcon;

	public UILabel LabelResultItemCount;

	public UILabel LabelRemainTime;

	public UILabel LabelItemName;

	public UILabel LabelItemDesc;

	public UILabel MainStatTypeLabel;

	public UILabel MainStatValueLabel;

	public UILabel Label_EquipLimit;

	public UILabel Label_EquipLevel;

	public UILabel Label_Class;

	public UIWidget UniqueWidget;

	public UITexture UniqueHeroTexture;

	public UILabel Label_CreatureLimit;

	public UISimpleGrid ClassTagGrid;

	private List<ClassTagIconComponent> _classTagIconComponentList = new List<ClassTagIconComponent>();

	public UIButton Button_Desc;

	public UISprite Sprite_Selected;

	public UILabel Label_ItemCount;

	private Listener _listener;

	private int _craftIndex;

	private bool _selected;

	public ItemData _itemData { get; private set; }

	public void Open(Listener listener, int craftIndex, string itemCode)
	{
		_listener = listener;
		_craftIndex = craftIndex;
		_itemData = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
		_selected = false;
		SetupUI();
	}

	public void SetSelected(int craftIndex)
	{
		_selected = craftIndex == _craftIndex;
		UIHelper.EnableGameObject(Sprite_Selected, _selected);
	}

	private void SetupUI()
	{
		if (_itemData != null)
		{
			FactoryBase.SafeReleaseComponent(ref ItemIcon);
			ItemIcon = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
			ItemIcon.Open(_itemData, this);
			UIHelper.SetLabelText(LabelItemName, _itemData.Name);
			UIHelper.SetLabelText(LabelItemDesc, _itemData.Description2);
			ActivateEquipDesc(_itemData.Type == ItemType.Equip);
			if (_itemData.Type == ItemType.Equip)
			{
				SetupEquipItem(_itemData);
				UIHelper.SetLabelText(Label_ItemCount, LocalizationManager.GetStringDirect("MISSION_SUBTYPE_ITEM"));
			}
			else
			{
				int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(_itemData.Code);
				string text = LocalizationManager.GetString(Item.COMMON_GROWTH_REWARD_COUNT, itemCount);
				UIHelper.SetLabelText(Label_ItemCount, text);
			}
		}
		UIHelper.EnableGameObject(Sprite_Selected, _selected);
		CraftItemData data = GameRoot.Instance.CraftItemDataContainer.GetData(_craftIndex);
		if (data != null)
		{
			TimeSpan timeSpan = new TimeSpan(0, 0, data.ReqTime);
			int num = (int)timeSpan.TotalHours;
			string text2 = "";
			UIHelper.SetLabelText(text: (num >= 100) ? $"{num:D3}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}" : $"{num:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}", label: LabelRemainTime);
			UIHelper.SetLabelText(LabelResultItemCount, data.ResultItemCount.ToString());
		}
	}

	private void SetupClassTagIcon(EquipItemData itemData)
	{
		if (ClassTagGrid == null || itemData == null)
		{
			return;
		}
		CreatureTagType[] tagType = itemData.TagType;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(tagType))
		{
			return;
		}
		if (GameRoot.Instance.EquipItemDataContainer.ContainsAllClassTag(itemData.EquipCode))
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			_classTagIconComponentList.Add(classTagIconComponent);
			classTagIconComponent.OpenAllClassTag(null);
			UIHelper.RepositionGrid(ClassTagGrid);
			return;
		}
		for (int num = tagType.Length - 1; num >= 0; num--)
		{
			CreatureTagType tagType2 = tagType[num];
			ClassTagIconComponent classTagIconComponent2 = UIHelper.AddGridItem(ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			if (!(classTagIconComponent2 == null))
			{
				_classTagIconComponentList.Add(classTagIconComponent2);
				classTagIconComponent2.Open(tagType2, null);
			}
		}
		UIHelper.RepositionGrid(ClassTagGrid);
	}

	private void SetupUniqueWidget(int heroIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		UIHelper.SetLabelText(Label_CreatureLimit, data.Name);
		AssetTexture heroIcon = UIHelper.GetHeroIcon2(heroIndex, 3);
		UIHelper.ResetPlaneAssetTextureAsync(this, UniqueHeroTexture, heroIcon);
	}

	private void SetupUniqueAndClass(EquipItemData itemData)
	{
		if (itemData != null)
		{
			bool flag = !__Extension_Method_ICollection__.IsNullOrEmpty(itemData.CreatureIndex);
			if (flag)
			{
				SetupUniqueWidget(itemData.CreatureIndex[0]);
			}
			else
			{
				SetupClassTagIcon(itemData);
			}
			UIHelper.EnableGameObject(ClassTagGrid, !flag);
			UIHelper.EnableGameObject(UniqueWidget, flag);
		}
	}

	public void ActivateEquipDesc(bool Active)
	{
		UIHelper.EnableGameObject(LabelItemDesc, enable: true);
		UIHelper.EnableGameObject(MainStatTypeLabel, Active);
		UIHelper.EnableGameObject(MainStatValueLabel, Active);
		UIHelper.EnableGameObject(Label_EquipLimit, Active);
		UIHelper.EnableGameObject(Label_EquipLevel, Active);
		UIHelper.EnableGameObject(Label_Class, Active);
		UIHelper.EnableGameObject(UniqueWidget, Active);
		UIHelper.EnableGameObject(UniqueHeroTexture, Active);
		UIHelper.EnableGameObject(Label_CreatureLimit, Active);
		UIHelper.EnableGameObject(ClassTagGrid, Active);
		UIHelper.EnableGameObject(Button_Desc, Active);
	}

	private void SetupEquipItem(ItemData itemData)
	{
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemData.Code);
		if (data != null)
		{
			FactoryBase.ReleaseAndClearList(_classTagIconComponentList);
			long statValue = 0L;
			data.SubData.GetMainStat(out var statType, out statValue, 0, data.SubData.DetailIndex);
			SetupUniqueAndClass(data.SubData);
			UIHelper.SetLabelText(Label_EquipLevel, (data.SubData.ReqLevel <= 0) ? 1 : data.SubData.ReqLevel);
			UIHelper.SetLabelText(MainStatTypeLabel, statType);
			UIHelper.SetLabelText(MainStatValueLabel, statValue);
		}
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(_craftIndex);
		}
	}

	public void OnDescClick()
	{
		if (_listener != null)
		{
			_listener.OnItemDescClick(_craftIndex);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent comp)
	{
		OnClick();
	}
}
