using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class TeamLevelTotalBuffPopup : BaseWindow, TeamLevelTotalBuffGroupComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/TeamLevelBuff/TeamLevelTotalBuffPopup";

	public UILabel LabelBuffCount;

	public UITable StatTable;

	public UIScrollView StatScrollView;

	private List<TeamLevelTotalBuffGroupComponent> _listGroupComp = new List<TeamLevelTotalBuffGroupComponent>();

	public static void OpenPopup()
	{
		WindowPrefab<TeamLevelTotalBuffPopup> windowPrefab = new WindowPrefab<TeamLevelTotalBuffPopup>("Resources/Prefab2/Window/TeamLevelBuff/TeamLevelTotalBuffPopup", GameUIRoot.ELayerType.UI);
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
		List<DefaultExtraStatData> teamLevelBuffStatDataBaseList = GlobalAutoBehaviour<TeamLevelBuffManager>.instance.GetTeamLevelBuffStatDataBaseList(sort: true);
		AddGroupComponent("PET_POPUP_DESC_COMMON_ABILITY", TeamLevelBuffManager.EBuffType.CommonBuff, teamLevelBuffStatDataBaseList);
		List<PetStatDataBase> teamLevelBuffContentsStatDataBaseList = GlobalAutoBehaviour<TeamLevelBuffManager>.instance.GetTeamLevelBuffContentsStatDataBaseList(null, sumEqualConditionOption: true, sort: true);
		AddGroupComponent("PET_POPUP_DESC_CONTENTS_ABILITY", TeamLevelBuffManager.EBuffType.ContentsBuff, teamLevelBuffContentsStatDataBaseList);
		List<PetBonusDataBase> teamLevelBonusDataBaseList = GlobalAutoBehaviour<TeamLevelBuffManager>.instance.GetTeamLevelBonusDataBaseList(sort: true);
		AddGroupComponent("PET_POPUP_DESC_CONVENIENCE_ABILITY", TeamLevelBuffManager.EBuffType.Bonus, teamLevelBonusDataBaseList);
		StatTable.Reposition();
		StatScrollView.ResetPosition();
		int num = __Extension_Method_ICollection__.COUNT(teamLevelBuffStatDataBaseList) + __Extension_Method_ICollection__.COUNT(teamLevelBuffContentsStatDataBaseList) + __Extension_Method_ICollection__.COUNT(teamLevelBonusDataBaseList);
		string stringDirect = LocalizationManager.GetStringDirect("PET_POPUP_DESC_ABILITY_APPLY", num);
		UIHelper.SetLabelText(LabelBuffCount, stringDirect);
	}

	private void AddGroupComponent<T>(string titleKey, TeamLevelBuffManager.EBuffType petBuffType, List<T> datas)
	{
		if (__Extension_Method_ICollection__.COUNT(datas) != 0)
		{
			TeamLevelTotalBuffGroupComponent teamLevelTotalBuffGroupComponent = TeamLevelTotalBuffGroupComponent.PrefabAuto.Factory.Create(StatTable.transform);
			if (!(teamLevelTotalBuffGroupComponent == null))
			{
				teamLevelTotalBuffGroupComponent.Open(this, titleKey, petBuffType, datas);
				_listGroupComp.Add(teamLevelTotalBuffGroupComponent);
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

	void TeamLevelTotalBuffGroupComponent.IListener.OnClickFoldOut(TeamLevelBuffManager.EBuffType teamLevelBuffType)
	{
		if (StatTable != null)
		{
			StatTable.Reposition();
		}
		GlobalAutoBehaviour<TeamLevelBuffManager>.instance.ClearChangedBuffInfo(teamLevelBuffType);
	}

	private void ReleaseComp()
	{
		FactoryBase.ReleaseAndClearList(_listGroupComp);
	}
}
