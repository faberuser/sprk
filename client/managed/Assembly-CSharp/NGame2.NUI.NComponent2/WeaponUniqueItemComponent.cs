using System.Collections.Generic;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class WeaponUniqueItemComponent : UIScrollItemBehaviour, ItemIconComponent.Listener, InitOnCreate
{
	public interface Listener
	{
		void OnClick(ItemData itemData);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/ShopItem/WeaponUniqueItemComponent";

	public static PrefabAuto<WeaponUniqueItemComponent> PrefabAuto = new PrefabAuto<WeaponUniqueItemComponent>("Resources/Prefab2/Component/ShopItem/WeaponUniqueItemComponent");

	public UISprite Sprite_Selected;

	public UISimpleGrid ItemGrid;

	public UILabel Label_ItemName;

	private ItemIconComponent _ItemIconComponent;

	[SerializeField]
	private UICircleTexture TexHero;

	[SerializeField]
	private UILabel LbHeroName;

	[SerializeField]
	private UISimpleGrid GridTagTypeIcon;

	private List<ClassTagIconComponent> _listTagTypeIconComponent = new List<ClassTagIconComponent>();

	private Listener _listener;

	private ItemData _itemData;

	private bool _selected;

	private int Star { get; set; }

	private int Level { get; set; }

	public void Open(ItemData itemData, int star, int level, Listener listener)
	{
		if (itemData != null)
		{
			_listener = listener;
			_itemData = itemData;
			_selected = false;
			Star = star;
			Level = level;
			Setup();
		}
	}

	public void SetSelected(int index)
	{
		if (_itemData != null)
		{
			_selected = index == _itemData.Index;
			UIHelper.EnableGameObject(Sprite_Selected, _selected);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}

	public void Close()
	{
		Reset();
	}

	private void Reset()
	{
		FactoryBase.ReleaseComponent(_ItemIconComponent);
		_ItemIconComponent = null;
	}

	public void OnClickItem()
	{
		if (_listener != null)
		{
			_listener.OnClick(_itemData);
		}
	}

	private void Setup()
	{
		SetupBackground();
		SetupItemIcon();
		SetHeroInfo();
		SetTagTypeIcon();
	}

	private void SetupBackground()
	{
		UIHelper.EnableGameObject(Sprite_Selected, _selected);
	}

	private void SetupItemIcon()
	{
		if (_ItemIconComponent == null)
		{
			_ItemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
			_ItemIconComponent.Open(_itemData, this);
			if (Star > 0)
			{
				_ItemIconComponent.SetStar(Star);
			}
		}
		UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(_itemData));
	}

	private void SetHeroInfo()
	{
		UIHelper.EnableGameObject(TexHero, enable: false);
		UIHelper.EnableGameObject(LbHeroName, enable: false);
		if (_itemData == null)
		{
			return;
		}
		EquipItemData data = GameRoot.Instance.EquipItemDataContainer.GetData(_itemData.Code);
		if (data != null && !__Extension_Method_ICollection__.IsNullOrEmpty(data.CreatureIndex) && data.CreatureIndex.Length != 0)
		{
			int num = data.CreatureIndex[0];
			CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(num);
			if (data2 != null)
			{
				UIHelper.EnableGameObject(LbHeroName, enable: true);
				UIHelper.SetLabelText(LbHeroName, data2.Name);
			}
			AssetTexture heroIcon = UIHelper.GetHeroIcon2(num, 3);
			UIHelper.ResetPlaneAssetTextureAsync(this, TexHero, heroIcon);
			UIHelper.EnableGameObject(TexHero, enable: true);
			UIHelper.EnableGameObject(LbHeroName, enable: true);
		}
	}

	private void SetTagTypeIcon()
	{
		FactoryBase.ReleaseAndClearList(_listTagTypeIconComponent);
		if (_itemData == null)
		{
			return;
		}
		EquipItemData data = GameRoot.Instance.EquipItemDataContainer.GetData(_itemData.Code);
		if (data == null)
		{
			return;
		}
		if (GameRoot.Instance.EquipItemDataContainer.ContainsAllClassTag(data.EquipCode))
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(GridTagTypeIcon, ClassTagIconComponent.PrefabAuto);
			if (classTagIconComponent != null)
			{
				_listTagTypeIconComponent.Add(classTagIconComponent);
				classTagIconComponent.OpenAllClassTag(null);
			}
		}
		else if (!__Extension_Method_ICollection__.IsNullOrEmpty(data.TagType) && __Extension_Method_ICollection__.IsNullOrEmpty(data.CreatureIndex))
		{
			CreatureTagType[] tagType = data.TagType;
			foreach (CreatureTagType tagType2 in tagType)
			{
				ClassTagIconComponent classTagIconComponent2 = UIHelper.AddGridItem(GridTagTypeIcon, ClassTagIconComponent.PrefabAuto);
				if (classTagIconComponent2 != null)
				{
					_listTagTypeIconComponent.Add(classTagIconComponent2);
					classTagIconComponent2.Open(tagType2, null);
				}
			}
		}
		UIHelper.RepositionGrid(GridTagTypeIcon);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		OnClickItem();
	}
}
