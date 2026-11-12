using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NAttendance;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenAccumulateLogin : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenAccumulateLogin();
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<AccumulateLogin>().Open(GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.LoginDailyCount, is_in_AttendanceIntegration: false);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
