using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBooster;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NHeroCostumeView;

[Serializable]
public class DisplayAllCostumeAbilities
{
	public UISimpleGrid BoosterGrid;

	private List<BoosterDescComponent> _abilityList = new List<BoosterDescComponent>();

	public void Reset()
	{
		ClearAll();
	}

	public void Refresh()
	{
		ClearAll();
		List<BoosterInfo> normalCostumeBoosterInfoList = GlobalAutoBehaviour<BoosterManager>.instance.GetNormalCostumeBoosterInfoList();
		if (__Extension_Method_ICollection__.COUNT(normalCostumeBoosterInfoList) == 0)
		{
			return;
		}
		foreach (BoosterInfo item in normalCostumeBoosterInfoList)
		{
			BoosterDescComponent boosterDescComponent = UIHelper.AddGridItem(BoosterGrid, BoosterDescComponent.PrefabAuto);
			boosterDescComponent.Open(item);
			_abilityList.Add(boosterDescComponent);
		}
	}

	private void ClearAll()
	{
		FactoryBase.ReleaseAndClearList(_abilityList);
	}
}
