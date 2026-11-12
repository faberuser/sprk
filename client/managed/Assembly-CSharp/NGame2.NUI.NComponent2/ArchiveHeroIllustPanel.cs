using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ArchiveHeroIllustPanel : MonoBehaviour, IHeroPanel, IArchiveHeroSubPanel, HeroPortraitComponent.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Component/Hero/ArchiveHeroIllustPanel";

	public static PrefabAuto<HeroPortraitComponent> PrefabAuto = new PrefabAuto<HeroPortraitComponent>("Resources/Prefab2/Component/Hero/ArchiveHeroIllustPanel");

	public UIScrollGrid Grid;

	private List<HeroPortraitComponent> componentList = new List<HeroPortraitComponent>();

	private CachedHeroInfo CachedHeroInfo;

	private void Init(CachedHeroInfo cachedHeroInfo)
	{
		base.gameObject.SetActive(value: true);
		Reset();
		CachedHeroInfo = cachedHeroInfo;
		foreach (CostumeData allDatum in GameRoot.Instance.CostumeDataContainer.GetAllData())
		{
			if (allDatum.HeroIndex == CachedHeroInfo.HeroIndex && allDatum.IsOpen && !String_Extension_Method.IsNullOrEmpty(allDatum.IllustPath))
			{
				HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(Grid, HeroPortraitComponent.SmallPrefabAuto);
				heroPortraitComponent.Open(cachedHeroInfo.CreatureData, this, HeroPortraitComponent.EPortraitMode.Icon, 0, 0, allDatum.CostumeIndex);
				heroPortraitComponent.ShowStar(show: false);
				heroPortraitComponent.SetLevel(show: false);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroPortraitComponent.transform);
				componentList.Add(heroPortraitComponent);
			}
		}
		UIHelper.RepositionGrid(Grid);
	}

	private void Reset()
	{
		FactoryBase.ReleaseAndClearList(componentList);
	}

	public void Enable(bool enable, CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		if (enable)
		{
			Init(cachedHeroInfo);
		}
		else
		{
			Close();
		}
	}

	public void Change(CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		Init(cachedHeroInfo);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public void OnClick(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait == null)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (HeroPortraitComponent component in componentList)
		{
			if (!component.IsDisabled)
			{
				list.Add(component.HeroCostumeIndex);
			}
		}
		SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.OpenIllustBook(heroPortrait.HeroCostumeIndex, list);
	}

	public void OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	public void OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	public void OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}
}
