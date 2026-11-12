using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class TreasureHouseProbabilityPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/TreasureHouse/TreasureHouseProbabilityPopup";

	public UIScrollGrid ScrollGrid;

	private List<TreasureHouseProbabilityComponent> _listComp = new List<TreasureHouseProbabilityComponent>();

	public static void Open(int removeIndex)
	{
		WindowPrefab<TreasureHouseProbabilityPopup> windowPrefab = new WindowPrefab<TreasureHouseProbabilityPopup>("Resources/Prefab2/Window/TreasureHouse/TreasureHouseProbabilityPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(removeIndex);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(int removeIndex)
	{
		Open(delegate
		{
			Setup(removeIndex);
		}, null);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseComponent();
	}

	private void Setup(int removeIndex)
	{
		int num = 0;
		int num2 = 0;
		TreasureHouseDungeonData[] array = GameRoot.Instance.TreasureHouseDungeonDataContainer.GetAllData().ToArray();
		Array.Sort(array, (TreasureHouseDungeonData lhs, TreasureHouseDungeonData rhs) => lhs.ShowIndex.CompareTo(rhs.ShowIndex));
		for (int num3 = 0; num3 < __Extension_Method_ICollection__.COUNT(array); num3++)
		{
			TreasureHouseDungeonData treasureHouseDungeonData = array[num3];
			if (treasureHouseDungeonData != null)
			{
				num += treasureHouseDungeonData.Ratio;
				if (removeIndex != treasureHouseDungeonData.Index)
				{
					num2 += treasureHouseDungeonData.Ratio;
				}
			}
		}
		for (int num4 = 0; num4 < __Extension_Method_ICollection__.COUNT(array); num4++)
		{
			TreasureHouseDungeonData treasureHouseDungeonData2 = array[num4];
			if (treasureHouseDungeonData2 != null)
			{
				string creatureName = GetCreatureName(treasureHouseDungeonData2.CreatureIndex);
				int partialRatio = ((removeIndex != treasureHouseDungeonData2.Index) ? treasureHouseDungeonData2.Ratio : 0);
				CreateComponent(ScrollGrid, creatureName, treasureHouseDungeonData2.Ratio, num, partialRatio, num2);
			}
		}
		UIHelper.RepositionGrid(ScrollGrid);
	}

	private void ReleaseComponent()
	{
		FactoryBase.ReleaseAndClearList(_listComp);
	}

	private string GetCreatureName(int creatureIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex);
		if (data == null)
		{
			return "";
		}
		return data.Name;
	}

	private void CreateComponent(IGrid grid, string creatureName, int totalRatio, int total, int partialRatio, int partial)
	{
		TreasureHouseProbabilityComponent treasureHouseProbabilityComponent = UIHelper.AddGridItem(grid, TreasureHouseProbabilityComponent.PrefabAuto);
		if (!(treasureHouseProbabilityComponent == null))
		{
			treasureHouseProbabilityComponent.Open(creatureName, totalRatio, total, partialRatio, partial);
			_listComp.Add(treasureHouseProbabilityComponent);
		}
	}
}
