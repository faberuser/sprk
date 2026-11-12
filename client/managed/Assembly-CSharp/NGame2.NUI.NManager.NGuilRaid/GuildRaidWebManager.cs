using System;
using NGame2.NGuild;
using NGame2.NUI.NWindow;
using NShared;
using NShared.GetGuildRaidList;
using NShared.GetGuildRaidMemberScoreList;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NGuilRaid;

public class GuildRaidWebManager : BaseWebManager
{
	public void RequestGetGuildRaidList(Action<NShared.GetGuildRaidList.Response> successAction, Action<NShared.GetGuildRaidList.Response> failAction)
	{
		Request<NShared.GetGuildRaidList.Request, NShared.GetGuildRaidList.Response>(delegate(NShared.GetGuildRaidList.Response response)
		{
			if (response.Result == NShared.GetGuildRaidList.ResultType.GuildRaidInitializing)
			{
				if (failAction == null)
				{
					Vespa_System_Action_Extension_Method.fire(successAction, response);
				}
				else
				{
					MessagePopup.OpenError(response.Result, delegate
					{
						if (failAction != null)
						{
							Vespa_System_Action_Extension_Method.fire(failAction, response);
						}
					});
				}
			}
			else if (response.Result != NShared.GetGuildRaidList.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildRaidManager.Apply(response.GuildRaidInfos);
				Vespa_System_Action_Extension_Method.fire(successAction, response);
			}
		});
	}

	public void RequestGuildRaidMemberScoreList(Action successAction)
	{
		NShared.GetGuildRaidMemberScoreList.Request request = new NShared.GetGuildRaidMemberScoreList.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildRaidMemberScoreList.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetGuildRaidMemberScoreList.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildRaidManager.Apply(response.GuildRaidMemberTotalScores);
				if (successAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(successAction);
				}
			}
		});
	}
}
