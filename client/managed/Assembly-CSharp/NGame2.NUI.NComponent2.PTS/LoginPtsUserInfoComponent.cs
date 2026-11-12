using System;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.PTS;

public class LoginPtsUserInfoComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Login/LoginPtsUserInfoComponent";

	public static PrefabAuto<LoginPtsUserInfoComponent> PrefabAuto = new PrefabAuto<LoginPtsUserInfoComponent>("Prefab2/Component/Login/LoginPtsUserInfoComponent");

	public UILabel RegionLabel;

	public UILabel NickNameLabel;

	public UILabel TeamLevelLabel;

	public UIButton Button_SelectUser;

	public UILabel Button_SelectUserLabel;

	private Action<string, int> _onClickButton;

	private int _accountId;

	private string _region = "-";

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(PtsUserInfo ptsUserInfo, Action<string, int> onClickButton)
	{
		Init(ptsUserInfo);
		_onClickButton = onClickButton;
	}

	private void Init(PtsUserInfo info)
	{
		_region = ((info.Region != null) ? info.Region.ToString() : "-");
		string text = "";
		string text2 = "";
		bool enable = false;
		_accountId = info.AccountId;
		string stringDirect;
		if (info.NickName != null && info.TeamLevel > 0)
		{
			text = info.NickName.ToString();
			text2 = "LV " + info.TeamLevel;
			if (info.IsEntry)
			{
				stringDirect = LocalizationManager.GetStringDirect("COMMON_ENTER");
				enable = true;
			}
			else
			{
				stringDirect = LocalizationManager.GetStringDirect("NOT_ENTRY_USER");
			}
		}
		else
		{
			stringDirect = LocalizationManager.GetStringDirect("NOT_EXIST_USER");
		}
		UIHelper.SetLabelText(RegionLabel, _region);
		UIHelper.SetLabelText(NickNameLabel, text);
		UIHelper.SetLabelText(TeamLevelLabel, text2);
		UIHelper.SetLabelText(Button_SelectUserLabel, stringDirect);
		UIHelper.EnableButton(Button_SelectUser, enable);
	}

	public void OnClickSelectUserButton()
	{
		if (_accountId > 0)
		{
			Vespa_System_Action_Extension_Method.fire(_onClickButton, _region, _accountId);
		}
	}
}
