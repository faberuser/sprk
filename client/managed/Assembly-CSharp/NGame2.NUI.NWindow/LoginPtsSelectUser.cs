using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2.PTS;
using NShared;
using NShared.GetUserInfoRegion;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class LoginPtsSelectUser : BaseWindow
{
	public const string PrefabPath = "Prefab2/Window/Login/LoginPtsSelectUser";

	private static GlobalWindowPrefab<LoginPtsSelectUser> _windowPrefab = new GlobalWindowPrefab<LoginPtsSelectUser>("Prefab2/Window/Login/LoginPtsSelectUser", GlobalUIRoot.ELayerType.GlobalUI);

	private List<LoginPtsUserInfoComponent> _userInfoComponentList = new List<LoginPtsUserInfoComponent>();

	public UILabel EntryConditionLabel;

	public UIGrid RewardGrid;

	private Action<string, int> _onSelectUser;

	public static void Open(NShared.GetUserInfoRegion.Response res, Action<string, int> onSelectUser)
	{
		_windowPrefab.instance.OpenInternal(res, onSelectUser);
	}

	protected void OpenInternal(NShared.GetUserInfoRegion.Response res, Action<string, int> onSelectUser)
	{
		base.Open();
		_onSelectUser = onSelectUser;
		Init(res.UserInfo, res.PtsEntryCondition);
	}

	public new static void Close()
	{
		_windowPrefab.instance.ClosePopup();
	}

	protected void ClosePopup()
	{
		ClearPtsUserInfoComponentList();
		base.Close();
	}

	private void Init(PtsUserInfo[] userData, PtsEntryCondition ptsEntryCondition)
	{
		int teamLevelValue = ptsEntryCondition.TeamLevelValue;
		string stringDirect = LocalizationManager.GetStringDirect(ptsEntryCondition.TeamLevelLocalization, teamLevelValue.ToString());
		string stringDirect2 = LocalizationManager.GetStringDirect("PTS_ENTRY_CONDITION_TEAM_TITLE", stringDirect);
		UIHelper.SetLabelText(EntryConditionLabel, stringDirect2);
		SetPtsUserInfoComponentList(userData);
	}

	private void ClearPtsUserInfoComponentList()
	{
		FactoryBase.ReleaseAndClearList(_userInfoComponentList);
	}

	private void SetPtsUserInfoComponentList(PtsUserInfo[] userData)
	{
		ClearPtsUserInfoComponentList();
		if (RewardGrid == null || __Extension_Method_ICollection__.IsNullOrEmpty(userData))
		{
			return;
		}
		LoginPtsUserInfoComponent loginPtsUserInfoComponent = null;
		PtsUserInfo ptsUserInfo = null;
		for (int i = 0; i < userData.Length; i++)
		{
			ptsUserInfo = userData[i];
			if (ptsUserInfo == null)
			{
				continue;
			}
			loginPtsUserInfoComponent = LoginPtsUserInfoComponent.PrefabAuto.Factory.Create(RewardGrid.transform);
			if (loginPtsUserInfoComponent == null)
			{
				continue;
			}
			loginPtsUserInfoComponent.Open(ptsUserInfo, delegate(string region, int accountId)
			{
				string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
				string stringDirect2 = LocalizationManager.GetStringDirect("PTS_USER_SELECT_POPUP_NOTIFICATION");
				GlobalMessagePopup.OpenGlobal(stringDirect, stringDirect2, delegate
				{
					Vespa_System_Action_Extension_Method.fire(_onSelectUser, region, accountId);
				}, null);
			});
			_userInfoComponentList.Add(loginPtsUserInfoComponent);
		}
		UIHelper.ResetGridPosition(RewardGrid);
	}
}
