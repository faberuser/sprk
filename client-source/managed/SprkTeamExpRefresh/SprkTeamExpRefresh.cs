using NGame2.NAccount;
using NShared;
using NShared.EndCampaign;

public static class SprkTeamExpRefresh
{
	public static void ApplyFinal(AccountManager account, NShared.EndCampaign.Response response)
	{
		if (account != null && response != null && response.ExpResult2 != null && account.UserManager != null)
		{
			account.UserManager.Apply(response.ExpResult2);
		}
	}

	public static ExpResultInfo BattleExp(NShared.EndCampaign.Response response)
	{
		object obj;
		if (response != null)
		{
			obj = response.ExpResult2;
			if (obj == null)
			{
				return response.ExpResult;
			}
		}
		else
		{
			obj = null;
		}
		return (ExpResultInfo)obj;
	}
}
