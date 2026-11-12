using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NHeroCostumeView;

[Serializable]
public class DisplayLegendCostumeAbilities
{
	public UISimpleGrid LegendBoosterGrid;

	private List<LegendBoosterDescComponent> _legendAbilityList = new List<LegendBoosterDescComponent>();

	public void Reset()
	{
		Clear();
	}

	public void Refresh()
	{
		Clear();
		if (GlobalAutoBehaviour<BoosterManager>.instance.GetEffectiveLegendCostumeBoosterCount() != 0)
		{
			LegendBoosterDescComponent legendBoosterDescComponent = UIHelper.AddGridItem(LegendBoosterGrid, LegendBoosterDescComponent.PrefabAuto);
			legendBoosterDescComponent.Open();
			_legendAbilityList.Add(legendBoosterDescComponent);
		}
	}

	private void Clear()
	{
		FactoryBase.ReleaseAndClearList(_legendAbilityList);
	}
}
