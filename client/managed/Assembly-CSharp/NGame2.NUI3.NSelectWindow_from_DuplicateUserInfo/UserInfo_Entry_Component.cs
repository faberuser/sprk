using NShared;
using UnityEngine;

namespace NGame2.NUI3.NSelectWindow_from_DuplicateUserInfo;

public class UserInfo_Entry_Component : MonoBehaviour
{
	public UILabel Label_Nickname;

	public UILabel Label_TeamLevel;

	public UILabel Label_RubyCount;

	public UserInfo UserInfo { get; private set; }

	public void InitUi(UserInfo userInfo)
	{
		UserInfo = userInfo;
		Label_Nickname.text = userInfo.Nick;
		Label_TeamLevel.text = $"Team Lv. {userInfo.TeamLevel}";
		Label_RubyCount.text = Vespa_Int_Extension_Method.ToCommaString(userInfo.Gem);
	}
}
