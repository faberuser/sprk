using System;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopEquipGachaHeroClassComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void OnClickTab(HighGachaCategoryType type);
	}

	[Serializable]
	public class GroupClass
	{
		public GameObject Root;

		public UISpriteSelector TagSpriteSelector;

		public UILabelSelector NameLabelSelector;

		public UIWidget ClassOnRoot;

		public UIWidget ClassOffRoot;

		public void SetClass(HighGachaCategoryType creatureTag)
		{
			SetClassName(creatureTag);
			bool flag = creatureTag != HighGachaCategoryType.NewHero && creatureTag != HighGachaCategoryType.All;
			if (flag)
			{
				TagSpriteSelector.Select(creatureTag.ToString());
			}
			EnableClassWidget(flag);
		}

		private void SetClassName(HighGachaCategoryType creatureTag)
		{
			NameLabelSelector.Select(creatureTag.ToString());
		}

		protected void EnableClassWidget(bool isClassOn)
		{
			UIHelper.EnableGameObject(ClassOnRoot, isClassOn);
			UIHelper.EnableGameObject(ClassOffRoot, !isClassOn);
		}
	}

	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopEquipGachaHeroClassComponent";

	public static PrefabAuto<PayShopEquipGachaHeroClassComponent> PrefabAuto = new PrefabAuto<PayShopEquipGachaHeroClassComponent>("Prefab2/Component/PayShop2/PayShopEquipGachaHeroClassComponent");

	[SerializeField]
	private GroupClass OnGroupClass;

	[SerializeField]
	private GroupClass OffGroupClass;

	private IListener _listener;

	public HighGachaCategoryType Type { get; private set; }

	public void Open(IListener listener, HighGachaCategoryType type)
	{
		_listener = listener;
		Type = type;
		SetClass(type);
	}

	public void SetClass(HighGachaCategoryType type)
	{
		OnGroupClass.SetClass(type);
		OffGroupClass.SetClass(type);
	}

	public void OnClickTab()
	{
		if (_listener != null)
		{
			_listener.OnClickTab(Type);
		}
	}

	public void Select(bool isSelect)
	{
		UIHelper.EnableGameObject(OnGroupClass.Root, isSelect);
		UIHelper.EnableGameObject(OffGroupClass.Root, !isSelect);
	}
}
