using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class HeroItemInfo : LinkFactoryComponent, InitOnCreate
{
	public static PrefabAuto<HeroItemInfo> PrefabAuto = new PrefabAuto<HeroItemInfo>("Prefab2/WorldMap/HeroItemInfo");

	public UIWidget BackgroundWidget;

	public Vector2 BackgroundCenterOffset;

	public Vector2 BackgroundSizeOffset;

	public UISimpleGrid HeroItemGrid;

	private List<HeroItemComponent> _heroIconList = new List<HeroItemComponent>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_heroIconList);
	}

	public void Open(IList<CreatureData> cretureDataList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(cretureDataList); i++)
		{
			CreatureData creatureData = cretureDataList[i];
			if (creatureData != null)
			{
				HeroItemComponent heroItemComponent = UIHelper.AddGridItem(HeroItemGrid, HeroItemComponent.PrefabAuto);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroItemComponent.transform);
				heroItemComponent.Open(creatureData);
				heroItemComponent.SetBorderImage(i == 0);
				_heroIconList.Add(heroItemComponent);
			}
		}
		UIHelper.RepositionGrid(HeroItemGrid);
	}
}
