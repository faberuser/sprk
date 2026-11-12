using System;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NUI.NManager.NBattle;
using NShared;
using NShared.EndCampaign;
using NShared.NJit;
using NVespa.NSingleton;

public static class SprkTeamExpRefreshProbe
{
	private sealed class Observer : IUserObserver<UserManager.UserStatInfo>, IUserObserver
	{
		public int ExpChanges;

		public int LevelChanges;

		public void OnChanged(UserManager.UserStatInfo info)
		{
			if (info.UserStatType == UserManager.UserStatType.TeamExp)
			{
				ExpChanges++;
			}
			if (info.UserStatType == UserManager.UserStatType.TeamLevel)
			{
				LevelChanges++;
			}
		}

		public bool IsGlobal()
		{
			return false;
		}
	}

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("Team EXP refresh: " + message);
		}
	}

	public static string Run()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		object value = instance.GetType().GetProperty("AccountManagers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance, null);
		FieldInfo field = value.GetType().GetField("UserManager");
		object value2 = field.GetValue(value);
		UserInfo obj = new UserInfo
		{
			TeamLevel = 100,
			TeamExp = 20608
		};
		int num = 1000;
		obj.Stamina = num;
		UserInfo userInfo = obj;
		UserManager userManager = new UserManager(instance, userInfo, new PlayerMiscInfo(), new PlayerGuildPointInfo());
		Observer observer = new Observer();
		userManager.ObserverManager.AddObserver(observer);
		field.SetValue(value, userManager);
		try
		{
			NShared.EndCampaign.Response response = JM_NShared_EndCampaign_Response.Parse("{\"ExpResult2\":{\"AddValue\":60,\"OldLevel\":100,\"NewLevel\":100,\"NewValue\":20668}}");
			Check(response.ExpResult == null && response.ExpResult2 != null, "native JSON parser receives settlement EXP");
			SprkTeamExpRefresh.ApplyFinal(instance, response);
			Check(userManager.TeamExp == 20668 && userManager.TeamLevel == 100, "same-level stamina EXP updates immediately");
			Check(observer.ExpChanges == 1, "native team EXP observers notified");
			Check(Helper.GetAccountTeamLevelInfo().CurExp == 20668, "profile/HUD level-info reads updated balance without login");
			EndCampaignProxy endCampaignProxy = new EndCampaignProxy();
			endCampaignProxy.OnResponse(new NShared.EndCampaign.Request(), response);
			Check(endCampaignProxy.ExpResult == response.ExpResult2, "battle result uses settlement EXP");
			SprkTeamExpRefresh.ApplyFinal(instance, response);
			Check(userManager.TeamExp == 20668 && observer.ExpChanges == 1, "replaying absolute snapshot does not add EXP twice");
			response.ExpResult = new ExpResultInfo
			{
				OldLevel = 100,
				NewLevel = 100,
				NewValue = 31790,
				AddValue = 10
			};
			response.ExpResultsByGetHero = new ExpResultInfo[1]
			{
				new ExpResultInfo
				{
					OldLevel = 100,
					NewLevel = 100,
					NewValue = 31810,
					AddValue = 20
				}
			};
			response.ExpResult2 = new ExpResultInfo
			{
				OldLevel = 100,
				NewLevel = 101,
				NewValue = 34,
				AddValue = 60
			};
			userManager.Apply(response.ExpResult);
			userManager.Apply(response.ExpResultsByGetHero);
			SprkTeamExpRefresh.ApplyFinal(instance, response);
			Check(userManager.TeamLevel == 101 && userManager.TeamExp == 34, "rollover and recruitment cannot overwrite final balance");
			Check(observer.LevelChanges == 1, "native level observers notified");
			endCampaignProxy.OnResponse(new NShared.EndCampaign.Request(), response);
			Check(endCampaignProxy.ExpResult.NewLevel == 101, "native battle proxy exposes level-up");
			int teamExp = userManager.TeamExp;
			response.ExpResult2 = null;
			SprkTeamExpRefresh.ApplyFinal(instance, response);
			Check(userManager.TeamExp == teamExp, "no stamina EXP on loss/no settlement is a no-op");
			endCampaignProxy.OnResponse(new NShared.EndCampaign.Request(), response);
			Check(endCampaignProxy.ExpResult == response.ExpResult, "legacy primary EXP fallback retained");
			SprkTeamExpRefresh.ApplyFinal(instance, null);
			SprkTeamExpRefresh.ApplyFinal(null, response);
			Check(SprkTeamExpRefresh.BattleExp(null) == null, "null response safe");
			return "PASS: native response parsing, same-level EXP, HUD/profile balance, observer notifications, level rollover, recruitment ordering, battle level-up proxy, replay idempotence, missing settlement and primary fallback.";
		}
		finally
		{
			field.SetValue(value, value2);
		}
	}
}
