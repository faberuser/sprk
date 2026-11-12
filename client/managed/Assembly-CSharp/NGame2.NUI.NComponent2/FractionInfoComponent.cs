using System.Collections.Generic;
using NGame2.NAccount;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class FractionInfoComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/UpperDungeon/FractionInfoComponent";

	public static PrefabAuto<FractionInfoComponent> PrefabAuto = new PrefabAuto<FractionInfoComponent>("Prefab2/Component/UpperDungeon/FractionInfoComponent");

	public UISpriteSelector ClassSelector;

	public UISimpleGrid FractionItemGrid;

	private List<ItemIconComponent> _fractionItemList = new List<ItemIconComponent>();

	public void Open(CreatureTagType tagType)
	{
		FactoryBase.ReleaseAndClearList(_fractionItemList);
		ClassSelector.SelectEnum(tagType);
		CreatureAwakeChallengeItemData data = GameRoot.Instance.CreatureAwakeChallengeItemDataContainer.GetData(tagType);
		if (data != null)
		{
			for (int i = 1; i <= 3; i++)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.GetItemCode(i));
				int itemCount = Helper.GetItemCount(dataByCode.Index);
				long count = itemCount;
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(FractionItemGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(dataByCode);
				itemIconComponent.SetItemCount(count);
				_fractionItemList.Add(itemIconComponent);
			}
			UIHelper.RepositionGrid(FractionItemGrid);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_fractionItemList);
	}
}
