using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBooster;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class LegendCostumeTotalBuffPopup : BaseWindow, LegendCostumeTotalBuffGroupComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/LegendCostume/LegendCostumeTotalBuffPopup";

	public UILabel LabelBuffCount;

	public UITable StatTable;

	public UIScrollView StatScrollView;

	private List<LegendCostumeTotalBuffGroupComponent> _listGroupComp = new List<LegendCostumeTotalBuffGroupComponent>();

	public static void OpenPopup()
	{
		WindowPrefab<LegendCostumeTotalBuffPopup> windowPrefab = new WindowPrefab<LegendCostumeTotalBuffPopup>("Resources/Prefab2/Window/LegendCostume/LegendCostumeTotalBuffPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal();
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal()
	{
		base.Open();
		Init();
	}

	private void Init()
	{
		List<BoosterInfo> allEffectiveLegendCostumeBoosterByType = GlobalAutoBehaviour<BoosterManager>.instance.GetAllEffectiveLegendCostumeBoosterByType(ECostumeBuffType.Bonus);
		AddGroupComponent("PET_POPUP_DESC_CONVENIENCE_ABILITY", ECostumeBuffType.Bonus, allEffectiveLegendCostumeBoosterByType);
		StatTable.Reposition();
		StatScrollView.ResetPosition();
		int num = __Extension_Method_ICollection__.COUNT(allEffectiveLegendCostumeBoosterByType);
		string stringDirect = LocalizationManager.GetStringDirect("PET_POPUP_DESC_ABILITY_APPLY", num);
		UIHelper.SetLabelText(LabelBuffCount, stringDirect);
	}

	private void AddGroupComponent(string titleKey, ECostumeBuffType costumeBuffType, List<BoosterInfo> datas)
	{
		if (__Extension_Method_ICollection__.COUNT(datas) != 0)
		{
			LegendCostumeTotalBuffGroupComponent legendCostumeTotalBuffGroupComponent = LegendCostumeTotalBuffGroupComponent.PrefabAuto.Factory.Create(StatTable.transform);
			if (!(legendCostumeTotalBuffGroupComponent == null))
			{
				legendCostumeTotalBuffGroupComponent.Open(this, titleKey, costumeBuffType, datas);
				_listGroupComp.Add(legendCostumeTotalBuffGroupComponent);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		ReleaseComp();
	}

	public void OnClickClose()
	{
		Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	void LegendCostumeTotalBuffGroupComponent.IListener.OnClickFoldOut(ECostumeBuffType costumeBuffType)
	{
		if (StatTable != null)
		{
			StatTable.Reposition();
		}
	}

	private void ReleaseComp()
	{
		FactoryBase.ReleaseAndClearList(_listGroupComp);
	}
}
