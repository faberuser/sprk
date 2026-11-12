using System;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopHeroClassTabComponent : PayShopBaseTabComponent<CreatureTagType>
{
	[Serializable]
	public class GroupClass
	{
		public UISpriteSelector TagSpriteSelector;

		public UILabel Label_Class;

		public UIWidget ClassOnRoot;

		public UIWidget ClassOffRoot;

		public void SetClass(CreatureTagType creatureTag)
		{
			SetClassName(creatureTag);
			if (creatureTag != CreatureTagType.None)
			{
				TagSpriteSelector.Select(creatureTag.ToString());
			}
			EnableClassWidget(creatureTag != CreatureTagType.None);
		}

		private void SetClassName(CreatureTagType creatureTag)
		{
			UIHelper.SetLabelText(text: (creatureTag != CreatureTagType.None) ? LocalizationManager.GetString(creatureTag) : LocalizationManager.GetStringDirect("COMMON_TOTAL"), label: Label_Class);
		}

		protected void EnableClassWidget(bool isClassOn)
		{
			UIHelper.EnableGameObject(ClassOnRoot, isClassOn);
			UIHelper.EnableGameObject(ClassOffRoot, !isClassOn);
		}
	}

	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopHeroClassTabComponent";

	public static PrefabAuto<PayShopHeroClassTabComponent> PrefabAuto = new PrefabAuto<PayShopHeroClassTabComponent>("Prefab2/Component/PayShop2/PayShopHeroClassTabComponent");

	[SerializeField]
	private GroupClass OnGroupClass;

	[SerializeField]
	private GroupClass OffGroupClass;

	public override void Open(IListener listener, CreatureTagType creatureTag)
	{
		base.Open(listener, creatureTag);
		SetClass(creatureTag);
	}

	public void SetClass(CreatureTagType creatureTag)
	{
		OnGroupClass.SetClass(creatureTag);
		OffGroupClass.SetClass(creatureTag);
	}
}
