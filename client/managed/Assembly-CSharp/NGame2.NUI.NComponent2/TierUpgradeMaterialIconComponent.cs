using NGame2.NAccount;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class TierUpgradeMaterialIconComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Icon/TierUpgradeMaterialIconComponent";

	public static PrefabAuto<TierUpgradeMaterialIconComponent> PrefabAuto = new PrefabAuto<TierUpgradeMaterialIconComponent>("Prefab2/Component/Icon/TierUpgradeMaterialIconComponent");

	public UIWidget Root;

	public Transform IconPostion;

	public UILabel ItemCountLabel;

	private ItemIconComponent Icon;

	private int TotalCount { get; set; }

	private int NeedCount { get; set; }

	public bool IsEnough => TotalCount >= NeedCount;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		UIHelper.SetWidgetGrayScale(Root, grayScale: false);
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		UIHelper.SetWidgetGrayScale(Root, grayScale: false);
		FactoryBase.SafeReleaseComponent(ref Icon);
	}

	public void Open(EquipUpgradeTierData.Material material)
	{
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(material.ItemCode);
		if (dataByCode != null)
		{
			ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconPostion);
			itemIconComponent.Open(dataByCode);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
			Icon = itemIconComponent;
			int count = 0;
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(dataByCode.Index);
			if (itemInfo != null)
			{
				count = itemInfo.Count;
			}
			SetCount(count, material.ItemCount);
		}
	}

	private void SetCount(int count, int need)
	{
		TotalCount = count;
		NeedCount = need;
		UIHelper.SetLabelText(ItemCountLabel, $"{TotalCount} / {NeedCount}");
		UIHelper.SetWidgetGrayScale(Root, !IsEnough);
	}
}
