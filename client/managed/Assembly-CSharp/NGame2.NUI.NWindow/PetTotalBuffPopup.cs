using System.Collections.Generic;
using NGame2.NPet;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class PetTotalBuffPopup : BaseWindow, PetTotalBuffGroupComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Pet/Popup/PetTotalBuffPopup";

	public UILabel LabelBuffCount;

	public UITable StatTable;

	public UIScrollView StatScrollView;

	private List<PetTotalBuffGroupComponent> _listGroupComp = new List<PetTotalBuffGroupComponent>();

	public static void OpenPopup()
	{
		WindowPrefab<PetTotalBuffPopup> windowPrefab = new WindowPrefab<PetTotalBuffPopup>("Resources/Prefab2/Window/Pet/Popup/PetTotalBuffPopup", GameUIRoot.ELayerType.UI);
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
		List<DefaultExtraStatData> petStatDataBaseList = GlobalAutoBehaviour<PetManager>.instance.GetPetStatDataBaseList(sort: true);
		AddGroupComponent("PET_POPUP_DESC_COMMON_ABILITY", PetManager.PetBuffType.CommonBuff, petStatDataBaseList);
		List<PetStatDataBase> petContentsStatDataBaseList = GlobalAutoBehaviour<PetManager>.instance.GetPetContentsStatDataBaseList(null, sumEqualConditionOption: true, sort: true);
		AddGroupComponent("PET_POPUP_DESC_CONTENTS_ABILITY", PetManager.PetBuffType.ContentsBuff, petContentsStatDataBaseList);
		List<PetBonusDataBase> petBonusDataBaseList = GlobalAutoBehaviour<PetManager>.instance.GetPetBonusDataBaseList(sort: true);
		AddGroupComponent("PET_POPUP_DESC_CONVENIENCE_ABILITY", PetManager.PetBuffType.Bonus, petBonusDataBaseList);
		StatTable.Reposition();
		StatScrollView.ResetPosition();
		int num = __Extension_Method_ICollection__.COUNT(petStatDataBaseList) + __Extension_Method_ICollection__.COUNT(petContentsStatDataBaseList) + __Extension_Method_ICollection__.COUNT(petBonusDataBaseList);
		string stringDirect = LocalizationManager.GetStringDirect("PET_POPUP_DESC_ABILITY_APPLY", num);
		UIHelper.SetLabelText(LabelBuffCount, stringDirect);
	}

	private void AddGroupComponent<T>(string titleKey, PetManager.PetBuffType petBuffType, List<T> datas)
	{
		if (__Extension_Method_ICollection__.COUNT(datas) != 0)
		{
			PetTotalBuffGroupComponent petTotalBuffGroupComponent = PetTotalBuffGroupComponent.PrefabAuto.Factory.Create(StatTable.transform);
			if (!(petTotalBuffGroupComponent == null))
			{
				petTotalBuffGroupComponent.Open(this, titleKey, petBuffType, datas);
				_listGroupComp.Add(petTotalBuffGroupComponent);
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

	void PetTotalBuffGroupComponent.IListener.OnClickFoldOut(PetManager.PetBuffType petBuffType)
	{
		if (StatTable != null)
		{
			StatTable.Reposition();
		}
		GlobalAutoBehaviour<PetManager>.instance.ClearChangedBuffInfo(petBuffType);
	}

	private void ReleaseComp()
	{
		FactoryBase.ReleaseAndClearList(_listGroupComp);
	}
}
