using System.Collections.Generic;
using NGame2.NUI.NComponent2.NCostume;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa.NNGUIExtension;

public static class SprkMotionCostumes
{
	public static void Setup(HeroInfo hero, CostumeData selected, UIBaseGrid grid, List<HeroCostumeComponent> components, HeroCostumeComponent.Listener listener)
	{
		if (hero == null)
		{
			return;
		}
		foreach (CostumeData allDatum in GameRoot.Instance.CostumeDataContainer.GetAllData())
		{
			if (allDatum != null && allDatum.HeroIndex == hero.HeroIndex)
			{
				HeroCostumeComponent heroCostumeComponent = grid.AddGridItem<HeroCostumeComponent>(HeroCostumeComponent.PrefabAuto);
				heroCostumeComponent.Open(allDatum, listener);
				components.Add(heroCostumeComponent);
			}
		}
		grid.RepositionGrid();
	}

	public static CachedHeroInfo Snapshot(CachedHeroInfo preview)
	{
		if (preview != null)
		{
			return JsonMarshaler.Clone(preview);
		}
		return null;
	}

	public static void Refresh(HeroDetailView window, HeroInfo hero, List<HeroCostumeComponent> components)
	{
		if (hero == null)
		{
			return;
		}
		foreach (HeroCostumeComponent component in components)
		{
			component.SetState(CostumeState.None);
			component.SetSelected(component.CostumeData != null && component.CostumeData.CostumeIndex == hero.CostumeIndex);
		}
	}
}
