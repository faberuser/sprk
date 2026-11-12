using System;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NWindow;
using NShared.GetGuildAttendance;
using NShared.SendGuildAttendanceReward;
using NShared.SetGuildAttendance;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NGuildAttendance;

public class GuildAttendanceWebManager : BaseWebManager
{
	public void RequestGetGuildAttendance(Action<NShared.GetGuildAttendance.Response> responseAction)
	{
		Request<NShared.GetGuildAttendance.Request, NShared.GetGuildAttendance.Response>(delegate(NShared.GetGuildAttendance.Response response)
		{
			if (response.Result != NShared.GetGuildAttendance.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.Apply(response.GuildAttendanceInfo);
				GlobalAutoBehaviour<GuildManager>.instance.Apply(response.GuildMemberAttendanceInfo);
				Vespa_System_Action_Extension_Method.fire(responseAction, response);
			}
		});
	}

	public void RequestSetGuildAttendance(Action<NShared.SetGuildAttendance.Response> responseAction)
	{
		Request<NShared.SetGuildAttendance.Request, NShared.SetGuildAttendance.Response>(delegate(NShared.SetGuildAttendance.Response response)
		{
			if (response.Result != NShared.SetGuildAttendance.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.Apply(response.GuildAttendanceInfo);
				GlobalAutoBehaviour<GuildManager>.instance.Apply(response.GuildMemberAttendanceInfo);
				GlobalAutoBehaviour<GuildManager>.instance.SetGuildAttendanceRewardLastTime(GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime());
				Vespa_System_Action_Extension_Method.fire(responseAction, response);
			}
		});
	}

	public void RequestSendGuildAttendanceReward(Action<NShared.SendGuildAttendanceReward.Response> responseAction)
	{
		Request<NShared.SendGuildAttendanceReward.Request, NShared.SendGuildAttendanceReward.Response>(delegate(NShared.SendGuildAttendanceReward.Response response)
		{
			if (response.Result != NShared.SendGuildAttendanceReward.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.Apply(response.GuildAttendanceInfo);
				GlobalAutoBehaviour<GuildManager>.instance.Apply(response.GuildMemberAttendanceInfo);
				Vespa_System_Action_Extension_Method.fire(responseAction, response);
			}
		});
	}
}
