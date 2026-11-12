using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NGame2.NUI.NManager;
using NGame2.NUtil;
using NShared;
using NShared.QueryMarketUrl;
using NVespa;
using NVespa.NSingleton;
using NVespa.NUtil;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.Networking;

public static class SprkNetworkPerformance
{
	private sealed class Delivery
	{
		public string Context;

		public string Progression;

		public string Catalog;

		public bool FullProgression;

		public bool FullCatalog;

		public string[] Sections;

		public bool[] FullSections;

		public WebServiceRequest Request;
	}

	private sealed class Timing
	{
		public long Queued;

		public long Sent;

		public long Processing;

		public long Parsed;
	}

	private static readonly object gate = new object();

	private static string scope;

	private static string token;

	private static string catalogToken;

	private static readonly string[] sectionNames = new string[4] { "Achievement", "MainQuest", "ClearMission", "PlayRecord" };

	private static readonly string[] sectionFields = new string[4] { "AchievementInfos", "ReservedMainQuestInfo", "ReservedClearMissionInfos", "ReservedPlayRecordInfos" };

	private static readonly string[] sectionHeaders = new string[4] { "X-SPRK-Achievements", "X-SPRK-Main-Quest", "X-SPRK-Clear-Missions", "X-SPRK-Play-Records" };

	private static readonly string[] sectionTokens = new string[4];

	private static readonly Regex[] sectionPatterns = Array.ConvertAll(sectionNames, (string n) => new Regex("\"Sprk" + n + "Token\"\\s*:\\s*\"([0-9a-f]{64})\""));

	private static readonly Regex tokenField = new Regex("\"SprkProgressionToken\"\\s*:\\s*\"([0-9a-f]{64})\"");

	private static readonly Regex catalogField = new Regex("\"SprkShopCatalogToken\"\\s*:\\s*\"([0-9a-f]{64})\"");

	private static readonly ConditionalWeakTable<WebServiceRequest, Timing> timings = new ConditionalWeakTable<WebServiceRequest, Timing>();

	private static readonly bool trace = Environment.GetEnvironmentVariable("SPRK_NETWORK_TRACE") == "1";

	private static readonly ConditionalWeakTable<object, Delivery> deliveries = new ConditionalWeakTable<object, Delivery>();

	private static readonly ConditionalWeakTable<object, HashSet<int>> noStaminaClock = new ConditionalWeakTable<object, HashSet<int>>();

	private static readonly HashSet<string> parallelReads = new HashSet<string>(StringComparer.Ordinal)
	{
		"ping", "ping/idle", "dispatch/get_dispatch_list", "mail/check_new_mail", "mail/get_mail_list", "mail/get_mail_list_by_page", "mail/get_global_mail_list", "mail/list", "friend/list", "friend/get_friend_list",
		"friend/search_friend", "match/get_season_info", "global_arena/get_season_info", "chat/info", "chat/recent", "query/session", "guild/get_guildbasicinfo", "guild/get_all_guild_member_info"
	};

	public const int MaxParallelReads = 4;

	private static List<WebServiceRequest> queue;

	private static readonly MethodInfo marketDone = typeof(QueryMarketInfo).GetProperty("IsDone").GetSetMethod(nonPublic: true);

	private static readonly MethodInfo marketResponse = typeof(QueryMarketInfo).GetProperty("Response").GetSetMethod(nonPublic: true);

	private static long Now => Stopwatch.GetTimestamp();

	private static double Ms(long ticks)
	{
		return (double)ticks * 1000.0 / (double)Stopwatch.Frequency;
	}

	private static void TraceLog(string message)
	{
		UnityEngine.Debug.Log(message);
		try
		{
			string text = Environment.GetEnvironmentVariable("SPRK_NETWORK_TRACE_FILE");
			if (string.IsNullOrEmpty(text))
			{
				text = Path.Combine(Application.persistentDataPath, "sprk-network.log");
			}
			lock (gate)
			{
				File.AppendAllText(text, message + Environment.NewLine);
			}
		}
		catch
		{
		}
	}

	public static void Started(WebServiceRequest request)
	{
		if (!trace || request == null)
		{
			return;
		}
		lock (gate)
		{
			if (timings.TryGetValue(request, out var value))
			{
				value.Sent = Now;
			}
		}
	}

	public static void BeginResponse(WebServiceRequest request)
	{
		if (!trace || request == null)
		{
			return;
		}
		lock (gate)
		{
			if (timings.TryGetValue(request, out var value))
			{
				value.Processing = Now;
			}
		}
	}

	public static void TransportComplete(UnityWebRequest request)
	{
		if (trace && request != null && request.isDone && string.IsNullOrEmpty(request.error))
		{
			Uri result;
			string text = (Uri.TryCreate(request.url, UriKind.Absolute, out result) ? result.AbsolutePath : "unknown");
			TraceLog("SPRK network transport path=" + text + " downloadedBytes=" + request.downloadedBytes + " encoding=" + (request.GetResponseHeader("Content-Encoding") ?? "identity") + " serverTiming=" + (request.GetResponseHeader("Server-Timing") ?? "unknown"));
		}
	}

	public static void SetStaminaClock(object manager, int type, int next)
	{
		if (manager == null || next < 0)
		{
			return;
		}
		lock (gate)
		{
			HashSet<int> orCreateValue = noStaminaClock.GetOrCreateValue(manager);
			if (next == 0)
			{
				orCreateValue.Add(type);
			}
			else
			{
				orCreateValue.Remove(type);
			}
		}
	}

	public static bool HasStaminaClock(object manager, int type)
	{
		if (manager == null)
		{
			return true;
		}
		lock (gate)
		{
			HashSet<int> value;
			return !noStaminaClock.TryGetValue(manager, out value) || !value.Contains(type);
		}
	}

	public static void Prepare(WebServiceRequest request, string session)
	{
		string text;
		using (SHA256 sHA = SHA256.Create())
		{
			text = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(request.Host + "\n" + session))).Replace("-", "");
		}
		Dictionary<string, string> dictionary = ((request.Headers == null) ? new Dictionary<string, string>() : new Dictionary<string, string>(request.Headers));
		dictionary.Remove("X-SPRK-Progression");
		dictionary.Remove("X-SPRK-Compact-Lobby");
		dictionary.Remove("X-SPRK-Shop-Catalog");
		string[] array = sectionHeaders;
		foreach (string key in array)
		{
			dictionary.Remove(key);
		}
		dictionary["X-SPRK-Sections"] = "1";
		dictionary["X-SPRK-Context"] = text;
		lock (gate)
		{
			if (scope != text)
			{
				scope = text;
				token = (catalogToken = null);
				Array.Clear(sectionTokens, 0, sectionTokens.Length);
			}
			if (!string.IsNullOrEmpty(token))
			{
				dictionary["X-SPRK-Progression"] = token;
			}
			if (!string.IsNullOrEmpty(catalogToken))
			{
				dictionary["X-SPRK-Shop-Catalog"] = catalogToken;
			}
			for (int j = 0; j < sectionTokens.Length; j++)
			{
				if (sectionTokens[j] != null)
				{
					dictionary[sectionHeaders[j]] = sectionTokens[j];
				}
			}
		}
		string text2 = request.ServiceName.Trim('/');
		if (text2 == "enter/lobby" || text2 == "lobby/enter_lobby")
		{
			dictionary["X-SPRK-Compact-Lobby"] = "1";
		}
		request.Headers = dictionary;
		if (trace)
		{
			lock (gate)
			{
				timings.Remove(request);
				timings.Add(request, new Timing
				{
					Queued = Now
				});
			}
		}
	}

	public static void Capture(string text, WebServiceRequest request, object response)
	{
		if (text == null || request == null || request.Headers == null || response == null || !request.Headers.TryGetValue("X-SPRK-Context", out var value))
		{
			return;
		}
		Match match = tokenField.Match(text);
		Match match2 = catalogField.Match(text);
		string[] array = new string[4];
		bool[] array2 = new bool[4];
		for (int i = 0; i < array.Length; i++)
		{
			Match match3 = sectionPatterns[i].Match(text);
			array[i] = (match3.Success ? match3.Groups[1].Value : null);
			array2[i] = text.Contains("\"" + sectionFields[i] + "\"");
		}
		lock (gate)
		{
			deliveries.Remove(response);
			deliveries.Add(response, new Delivery
			{
				Context = value,
				Request = request,
				Sections = array,
				FullSections = array2,
				Progression = (match.Success ? match.Groups[1].Value : null),
				Catalog = (match2.Success ? match2.Groups[1].Value : null),
				FullCatalog = text.Contains("\"PayShopProductInfos\""),
				FullProgression = (text.Contains("\"AchievementInfos\"") || text.Contains("\"ReservedMainQuestInfo\"") || text.Contains("\"ReservedClearMissionInfos\"") || text.Contains("\"ReservedPlayRecordInfos\""))
			});
			if (trace && timings.TryGetValue(request, out var value2))
			{
				value2.Parsed = Now;
			}
		}
	}

	public static void ProgressionApplied(object response)
	{
		if (response == null)
		{
			return;
		}
		lock (gate)
		{
			if (!deliveries.TryGetValue(response, out var value) || value.Context != scope)
			{
				return;
			}
			if (trace && timings.TryGetValue(value.Request, out var value2))
			{
				TraceLog(string.Format(CultureInfo.InvariantCulture, "SPRK network apply path={0} queueMs={1:F2} transportAndWorkerMs={2:F2} parseMs={3:F2} dispatchAndApplyMs={4:F2}", value.Request.ServiceName.Trim('/'), Ms(value2.Sent - value2.Queued), Ms(value2.Processing - value2.Sent), Ms(value2.Parsed - value2.Processing), Ms(Now - value2.Parsed)));
				timings.Remove(value.Request);
			}
			if (value.Progression != null)
			{
				token = value.Progression;
			}
			else if (value.FullProgression)
			{
				token = null;
			}
			if (value.FullCatalog && value.Catalog == null)
			{
				catalogToken = null;
			}
			for (int i = 0; i < sectionTokens.Length; i++)
			{
				if (value.Sections[i] != null)
				{
					sectionTokens[i] = value.Sections[i];
				}
				else if (value.FullSections[i])
				{
					sectionTokens[i] = null;
				}
			}
		}
	}

	public static void CatalogApplied(object response)
	{
		if (response == null)
		{
			return;
		}
		lock (gate)
		{
			if (deliveries.TryGetValue(response, out var value) && value.Context == scope)
			{
				catalogToken = value.Catalog;
			}
		}
	}

	public static void ShopListApplied(ShopListItemInfo[] items)
	{
		if (items == null || !SceneAutoBehaviour<ShopManagement>.isValidInstance)
		{
			return;
		}
		Dictionary<int, List<ShopItemInfo>> dictionary = new Dictionary<int, List<ShopItemInfo>>();
		foreach (ShopListItemInfo shopListItemInfo in items)
		{
			if (shopListItemInfo != null)
			{
				if (!dictionary.TryGetValue(shopListItemInfo.ShopIndex, out var value))
				{
					value = (dictionary[shopListItemInfo.ShopIndex] = new List<ShopItemInfo>());
				}
				value.Add(new ShopItemInfo
				{
					ShopIndex = shopListItemInfo.ShopIndex,
					ListNo = shopListItemInfo.ListNo,
					Sold = shopListItemInfo.Sold,
					Purchased = shopListItemInfo.Purchased
				});
			}
		}
		ShopManagement instance = SceneAutoBehaviour<ShopManagement>.instance;
		foreach (KeyValuePair<int, List<ShopItemInfo>> item in dictionary)
		{
			instance.SetShopItemInfos(item.Key, item.Value.ToArray());
		}
	}

	private static string Service(WebServiceRequest request)
	{
		if (request != null && request.ServiceName != null)
		{
			return request.ServiceName.Trim('/');
		}
		return "";
	}

	public static bool ParallelRead(WebServiceRequest request)
	{
		return parallelReads.Contains(Service(request));
	}

	public static void Pump(List<WebServiceRequest> list)
	{
		if (list == null)
		{
			return;
		}
		queue = list;
		double num = (double)WebServiceRequest.GetTimeout() + 5.0;
		DateTime utcNow = DateTime.UtcNow;
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			WebServiceRequest webServiceRequest = list[num2];
			if (webServiceRequest == null || (webServiceRequest.Started && (webServiceRequest.Ended || (utcNow - webServiceRequest.RequestedTime).TotalSeconds > num)))
			{
				list.RemoveAt(num2);
			}
		}
		int num3 = 0;
		for (int i = 0; i < list.Count; i++)
		{
			WebServiceRequest webServiceRequest2 = list[i];
			bool flag = ParallelRead(webServiceRequest2);
			if (webServiceRequest2.Started)
			{
				if (!flag)
				{
					break;
				}
				num3++;
				continue;
			}
			if (flag)
			{
				if (num3 >= 4)
				{
					break;
				}
				webServiceRequest2.Start();
				num3++;
				continue;
			}
			if (num3 == 0)
			{
				webServiceRequest2.Start();
			}
			break;
		}
	}

	public static bool Pending(string service)
	{
		List<WebServiceRequest> list = queue;
		if (list == null)
		{
			return false;
		}
		foreach (WebServiceRequest item in list)
		{
			if (item != null && (!item.Started || !item.Ended) && Service(item) == service)
			{
				return true;
			}
		}
		return false;
	}

	public static bool SkipMailCheck()
	{
		return Pending("mail/check_new_mail");
	}

	public static bool AnswerMarketQuery(QueryMarketInfo query, Action onComplete)
	{
		if (query == null)
		{
			return false;
		}
		NShared.QueryMarketUrl.Response response = new NShared.QueryMarketUrl.Response
		{
			BaseResult = BaseResultType.Success,
			Result = ResultType.Success
		};
		marketResponse.Invoke(query, new object[1] { response });
		marketDone.Invoke(query, new object[1] { true });
		if (onComplete != null)
		{
			SingletonUpdater<Executer>.instance.NextRun(onComplete);
		}
		return true;
	}
}
