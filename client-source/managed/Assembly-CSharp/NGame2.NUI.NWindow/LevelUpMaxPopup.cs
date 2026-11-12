using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class LevelUpMaxPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/LevelUpMaxPopup";

	public UILabel Label_Title;

	public UILabel Label_Content1;

	public UILabel Label_Content2;

	public UILabel Label_BeforeLevel;

	public UILabel Label_AfterLevel;

	public UISimpleGrid Grid_Before;

	public UISimpleGrid Grid_After;

	private Action _onYes;

	private Action _onCancel;

	private List<HeroPortraitComponent> _heroPortraitComponentList;

	public static void Open(CachedHeroInfo heroInfo, ItemInfo itemInfo, int expPotionCount, long totalExp, int changeLevel, Action onYes, Action onCancel)
	{
		WindowPrefab<LevelUpMaxPopup> windowPrefab = new WindowPrefab<LevelUpMaxPopup>("Resources/Prefab2/Window/MessagePopup/LevelUpMaxPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(heroInfo, itemInfo, expPotionCount, totalExp, changeLevel, onYes, onCancel);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(CachedHeroInfo heroInfo, ItemInfo itemInfo, int expPotionCount, long totalExp, int changeLevel, Action onYes, Action onCancel)
	{
		base.Open();
		_onYes = onYes;
		_onCancel = onCancel;
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("EXP_USE_POPUP_MESSAGE_1", data.Name, expPotionCount);
		string stringDirect2 = LocalizationManager.GetStringDirect("EXP_USE_POPUP_MESSAGE_2", totalExp);
		UIHelper.SetLabelText(Label_Title, text);
		UIHelper.SetLabelText(Label_Content1, stringDirect);
		UIHelper.SetLabelText(Label_Content2, stringDirect2);
		UIHelper.SetLabelText(Label_BeforeLevel, heroInfo.Level);
		UIHelper.SetLabelText(Label_AfterLevel, changeLevel);
		_heroPortraitComponentList = _heroPortraitComponentList ?? new List<HeroPortraitComponent>();
		AddHeroComponent(Grid_Before, heroInfo);
		AddHeroComponent(Grid_After, heroInfo);
	}

	private void AddHeroComponent(UISimpleGrid grid, CachedHeroInfo heroInfo)
	{
		HeroPortraitComponent heroPortraitComponent = grid.AddGridItem(HeroPortraitComponent.SmallPrefabAuto);
		heroPortraitComponent.Open(heroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
		heroPortraitComponent.SetLevel(show: false);
		_heroPortraitComponentList.Add(heroPortraitComponent);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_heroPortraitComponentList);
		_heroPortraitComponentList = null;
	}

	public void OnClickYes()
	{
		Vespa_System_Action_Extension_Method.fire(_onYes);
		Close();
	}

	public void OnClickCancel()
	{
		Vespa_System_Action_Extension_Method.fire(_onCancel);
		Close();
	}
}
