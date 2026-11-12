using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroSelectGuidePopup : BaseWindow
{
	public const string PrefabPath = "Prefab2/Window/Inventory/MultipleHeroSelect/HeroSelectGuidePopup";

	public UIButton Disable_Background;

	public GameObject DisableCheckedObject;

	private LocalStorageBool neverShowMeAgainThisGuide;

	private string guideKey;

	public override void Open()
	{
		base.Open(delegate
		{
		});
	}

	public void Open(string guideKey)
	{
		base.Open(delegate
		{
		});
		this.guideKey = guideKey;
		neverShowMeAgainThisGuide = new LocalStorageBool(i_default: false, typeof(HeroSelectGuidePopup).Name + guideKey);
		UIHelper.EnableGameObject(DisableCheckedObject, neverShowMeAgainThisGuide.Value);
	}

	public override void Close()
	{
		neverShowMeAgainThisGuide = null;
		base.Close();
	}

	public void OnClickNeverShowMeAgain()
	{
		bool flag = !UIHelper.IsEnabledGameObjet(DisableCheckedObject);
		UIHelper.EnableGameObject(DisableCheckedObject, flag);
		neverShowMeAgainThisGuide.Value = flag;
	}

	public void OnClickCloseWindow()
	{
		Close();
	}
}
