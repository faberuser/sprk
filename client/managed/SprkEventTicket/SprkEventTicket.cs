using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Event dungeon tickets are kept per event on the server (EventDungeonTicket,
/// one balance and daily recharge per event chapter). After login the client
/// loads every event's balance and keeps it current, so opening an event's
/// menu or dungeon list shows that event's count at once; the server is then
/// told which event is in use and its answer refreshes the count.
/// </summary>
public sealed class SprkEventTicket : MonoBehaviour
{
	private sealed class Cached
	{
		public StaminaResultInfo Info;

		public float At;
	}

	private static SprkEventTicket instance;

	private static readonly Dictionary<int, Cached> cache = new Dictionary<int, Cached>();

	/// <summary>The event the shown ticket count belongs to (0: unknown).</summary>
	private static int current;

	private static string scope = "";

	private static bool loading;

	private static float nextLoad;

	private static int requested;

	private static float requestedAt = -100f;

	private static bool Ready(out string host, out string sessionKey, out string accountScope)
	{
		host = sessionKey = accountScope = null;
		AccountManager accountManager = GlobalAutoBehaviour<AccountManager>.instance;
		WebServiceManager webServiceManager = SingletonUpdater<WebServiceManager>.instance;
		if (accountManager == null || !accountManager.LoginResponseComplete || accountManager.UserInfo == null || webServiceManager == null || webServiceManager.Requester == null || string.IsNullOrEmpty(webServiceManager.Requester.SessionKey))
		{
			return false;
		}
		host = webServiceManager.Requester.Host.TrimEnd('/');
		sessionKey = webServiceManager.Requester.SessionKey;
		accountScope = host + "|" + accountManager.UserInfo.AccountId + "|" + sessionKey;
		return true;
	}

	private static SprkEventTicket Instance()
	{
		if (instance == null)
		{
			GameObject obj = new GameObject("SprkEventTicket");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			instance = obj.AddComponent<SprkEventTicket>();
		}
		return instance;
	}

	/// <summary>Each frame: load every event's balance once per login session.</summary>
	public static void Tick()
	{
		if (!Ready(out var host, out var sessionKey, out var accountScope))
		{
			return;
		}
		if (accountScope != scope)
		{
			scope = accountScope;
			cache.Clear();
			current = 0;
			loading = false;
			nextLoad = 0f;
		}
		if (!loading && nextLoad >= 0f && Time.realtimeSinceStartup >= nextLoad)
		{
			loading = true;
			Instance().StartCoroutine(Instance().LoadAll(host, sessionKey, accountScope));
		}
	}

	/// <summary>
	/// UserManager.Apply: a ticket result belongs to the event in use, so keep it
	/// (battle starts and ends, recharges) as that event's balance.
	/// </summary>
	public static void Observe(StaminaResultInfo info)
	{
		if (info != null && info.Type == StaminaType.EventDungeonTicket && current > 0)
		{
			cache[current] = new Cached
			{
				Info = Copy(info),
				At = Time.realtimeSinceStartup
			};
		}
	}

	/// <summary>An entry menu button: the event chapter its action opens.</summary>
	public static void MenuButton(string[] button)
	{
		string action = (button != null && button.Length > 1) ? button[1] : null;
		int index = (action == "OpenEventDungeon") ? 2 : ((action == "OpenEventDungeonSelect") ? 3 : (-1));
		if (index > 0 && button.Length > index && int.TryParse(button[index], out var chapter))
		{
			Show(chapter);
		}
	}

	public static void Show(int chapter)
	{
		if (chapter <= 0 || !Ready(out var host, out var sessionKey, out var accountScope) || accountScope != scope)
		{
			return;
		}
		bool switched = chapter != current;
		current = chapter;
		if (switched && cache.TryGetValue(chapter, out var cached))
		{
			Apply(Aged(cached));
		}
		// The menu and its list open together; ask once.
		if (chapter == requested && Time.realtimeSinceStartup - requestedAt < 2f)
		{
			return;
		}
		requested = chapter;
		requestedAt = Time.realtimeSinceStartup;
		Instance().StartCoroutine(Instance().Fetch(host, sessionKey, accountScope, chapter));
	}

	private static StaminaResultInfo Copy(StaminaResultInfo info)
	{
		return new StaminaResultInfo
		{
			Type = info.Type,
			AddValue = 0L,
			NewValue = info.NewValue,
			StaminaRechargeTime = info.StaminaRechargeTime,
			NextRechargeRemainTime = info.NextRechargeRemainTime,
			FullRechargeRemainTime = info.FullRechargeRemainTime,
			RechargeCount = info.RechargeCount,
			IsHide = info.IsHide
		};
	}

	/// <summary>A kept balance with its recharge timers moved on to now.</summary>
	private static StaminaResultInfo Aged(Cached cached)
	{
		StaminaResultInfo info = Copy(cached.Info);
		int elapsed = Mathf.Max(0, Mathf.FloorToInt(Time.realtimeSinceStartup - cached.At));
		if (info.NextRechargeRemainTime > 0)
		{
			info.NextRechargeRemainTime = Mathf.Max(0, info.NextRechargeRemainTime - elapsed);
		}
		if (info.FullRechargeRemainTime > 0)
		{
			info.FullRechargeRemainTime = Mathf.Max(0, info.FullRechargeRemainTime - elapsed);
		}
		return info;
	}

	private static void Apply(StaminaResultInfo info)
	{
		AccountManager accountManager = GlobalAutoBehaviour<AccountManager>.instance;
		if (accountManager != null && accountManager.UserManager != null)
		{
			accountManager.UserManager.Apply(info);
		}
	}

	private static UnityWebRequest Post(string host, string body)
	{
		UnityWebRequest request = new UnityWebRequest(host + "/sprk/event_ticket", "POST");
		request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
		request.downloadHandler = new DownloadHandlerBuffer();
		request.timeout = 15;
		request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		return request;
	}

	private IEnumerator LoadAll(string host, string sessionKey, string accountScope)
	{
		bool success = false;
		try
		{
			using UnityWebRequest request = Post(host, "SessionKey=" + UnityWebRequest.EscapeURL(sessionKey));
			yield return request.SendWebRequest();
			if (request.isNetworkError || request.isHttpError || accountScope != scope)
			{
				yield break;
			}
			if (!(Json.Deserialize(request.downloadHandler.text) is IDictionary reply) || Convert.ToString(reply["Result"]) != "Success" || !(reply["Tickets"] is IList tickets))
			{
				yield break;
			}
			float now = Time.realtimeSinceStartup;
			foreach (object item in tickets)
			{
				if (item is IDictionary entry && entry["StaminaResult"] is IDictionary ticket)
				{
					int chapter = Convert.ToInt32(entry["ChapterIndex"]);
					// A balance seen meanwhile (a menu, a battle) is newer.
					if (!cache.ContainsKey(chapter))
					{
						cache[chapter] = new Cached
						{
							Info = Copy(JM_NShared_StaminaResultInfo.Parse(ticket)),
							At = now
						};
					}
				}
			}
			if (current == 0)
			{
				current = Convert.ToInt32(reply["Current"]);
			}
			success = true;
		}
		finally
		{
			if (accountScope == scope)
			{
				loading = false;
				// Once per session; retry a failed load.
				nextLoad = success ? (-1f) : (Time.realtimeSinceStartup + 10f);
			}
		}
	}

	private IEnumerator Fetch(string host, string sessionKey, string accountScope, int chapter)
	{
		using UnityWebRequest request = Post(host, "SessionKey=" + UnityWebRequest.EscapeURL(sessionKey) + "&ChapterIndex=" + chapter);
		yield return request.SendWebRequest();
		if (request.isNetworkError || request.isHttpError || accountScope != scope)
		{
			yield break;
		}
		try
		{
			if (Json.Deserialize(request.downloadHandler.text) is IDictionary reply && Convert.ToString(reply["Result"]) == "Success" && reply["StaminaResult"] is IDictionary ticket)
			{
				StaminaResultInfo info = JM_NShared_StaminaResultInfo.Parse(ticket);
				cache[chapter] = new Cached
				{
					Info = Copy(info),
					At = Time.realtimeSinceStartup
				};
				// Another event's menu may have opened meanwhile.
				if (chapter == current)
				{
					Apply(info);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SprkEventTicket: " + ex.Message);
		}
	}
}
