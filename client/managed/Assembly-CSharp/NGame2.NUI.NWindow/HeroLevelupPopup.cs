using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class HeroLevelupPopup : BaseWindow
{
	public class HeroLevelupInfo
	{
		public CachedHeroInfo HeroInfo;

		public int PrevHeroLevel;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/HeroLevelupPopup";

	public UISimpleGrid HeroLevelupListGrid;

	private List<HeroLevelupDescComponent> _HeroLevelupDescList = new List<HeroLevelupDescComponent>();

	private static WindowPrefab<HeroLevelupPopup> _windowPrefab;

	public static HeroLevelupPopup instance
	{
		get
		{
			if (_windowPrefab == null)
			{
				_windowPrefab = new WindowPrefab<HeroLevelupPopup>("Resources/Prefab2/Window/MessagePopup/HeroLevelupPopup", GameUIRoot.ELayerType.UI);
			}
			_windowPrefab.instance.gameObject.SetActive(value: false);
			return _windowPrefab.instance;
		}
	}

	public static void Open(List<HeroLevelupInfo> listHeroLevelupInfos)
	{
		if (_windowPrefab == null)
		{
			_windowPrefab = new WindowPrefab<HeroLevelupPopup>("Resources/Prefab2/Window/MessagePopup/HeroLevelupPopup", GameUIRoot.ELayerType.UI);
		}
		_windowPrefab.instance.OpenInternal(listHeroLevelupInfos);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_HeroLevelupDescList);
	}

	private void OpenInternal(List<HeroLevelupInfo> listHeroLevelupInfos)
	{
		base.Open();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(listHeroLevelupInfos); i++)
		{
			HeroLevelupDescComponent heroLevelupDescComponent = UIHelper.AddGridItem(HeroLevelupListGrid, HeroLevelupDescComponent.PrefabAuto);
			heroLevelupDescComponent.Open(listHeroLevelupInfos[i].HeroInfo, listHeroLevelupInfos[i].PrevHeroLevel);
			_HeroLevelupDescList.Add(heroLevelupDescComponent);
		}
		UIHelper.RepositionGrid(HeroLevelupListGrid);
	}

	public void OnClickClose()
	{
		Close();
	}
}
