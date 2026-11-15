using System;
using System.Linq;
using NGame2.NAccount;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

/// Achievement deltas on the real client: sends a gameplay mutation through the
/// native requester, so OnBaseResponse applies its reply exactly as in play,
/// then reports how many achievements the reply carried and the client's
/// resulting list (index:step:progress:team level), to compare with the
/// server's full list.
public static class SprkAchievementDeltaProbe
{
	public static string Status = "Not started";

	public static void Begin()
	{
		if (!(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE") ?? "").StartsWith("achievement-delta-"))
		{
			throw new InvalidOperationException("Achievement delta probe requires an isolated achievement-delta profile");
		}
		if (!AccountManager.IsLogined())
		{
			throw new InvalidOperationException("Achievement delta probe requires a logged-in account");
		}
		if (Status == "Running")
		{
			throw new InvalidOperationException("Achievement delta probe already running");
		}
		Status = "Running";
		var requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		var request = new NShared.BeginTutorial.Request { TutorialIndex = 1 };
		string reply = "";
		Action<NShared.Request, NShared.Response, string> listen = delegate(NShared.Request sent, NShared.Response response, string text)
		{
			if (sent == request) reply = text ?? "";
		};
		requester.Listen = (Action<NShared.Request, NShared.Response, string>)Delegate.Combine(requester.Listen, listen);
		NVespa.NWeb.WebServiceRequest pending = null;
		// Ping passes the service disablers a running tutorial installs.
		pending = requester.Request(request, NVespa.NWeb.WebRequestFlag.Ping, delegate(NShared.BeginTutorial.Response response)
		{
			requester.Listen = (Action<NShared.Request, NShared.Response, string>)Delegate.Remove(requester.Listen, listen);
			try
			{
				// Null when the reply omitted the list (unchanged since acknowledged).
				int carried = (response?.AchievementInfos == null) ? -1 : response.AchievementInfos.Length;
				var headers = pending?.Headers ?? new System.Collections.Generic.Dictionary<string, string>();
				string sent = string.Join(",", headers.Where(h => h.Key.StartsWith("X-SPRK-") && h.Key != "X-SPRK-Context")
					.Select(h => h.Key + "=" + (h.Value.Length > 8 ? h.Value.Substring(0, 8) : h.Value)));
				int tokenAt = reply.IndexOf("\"SprkAchievementToken\"");
				string token = tokenAt < 0 ? "none" : reply.Substring(tokenAt + 24, System.Math.Min(10, reply.Length - tokenAt - 24));
				Status = "DONE result=" + response?.Result + " base=" + response?.BaseResult + " carried=" + carried + " session=" + requester.SessionKey
					+ " list=" + List() + " sent=" + sent + " replyBytes=" + reply.Length + " replyToken=" + token;
			}
			catch (Exception error)
			{
				Status = "FAIL: " + error.Message;
			}
		});
	}

	private static string List()
	{
		var manager = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager;
		return string.Join(";", manager.GetValues().OrderBy(a => a.AchievementIndex)
			.Select(a => a.AchievementIndex + ":" + a.LastStep + ":" + a.Achievement + ":" + a.ClearTeamLevel));
	}
}
