using System;
using System.Collections;
using System.Linq;
using System.Text;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NUI.NManager.Mission;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.Networking;

public sealed class SprkMissionHistory : MonoBehaviour
{
	private static SprkMissionHistory instance;

	private static readonly SprkMissionHistoryState state = new SprkMissionHistoryState();

	private static int initialized = -1;

	private static bool busy;

	private static float nextAttempt;

	private static string Key(int main, int sub)
	{
		return main + ":" + sub;
	}

	private static bool Ready()
	{
		AccountManager accountManager = GlobalAutoBehaviour<AccountManager>.instance;
		WebServiceManager webServiceManager = SingletonUpdater<WebServiceManager>.instance;
		if (accountManager == null || !accountManager.LoginResponseComplete || accountManager.UserInfo == null || webServiceManager.Requester == null || string.IsNullOrEmpty(webServiceManager.Requester.SessionKey))
		{
			return false;
		}
		string scope = webServiceManager.Requester.Host.TrimEnd('/') + "|" + accountManager.UserInfo.AccountId + "|" + webServiceManager.Requester.SessionKey;
		if (state.SetScope(scope))
		{
			busy = false;
			nextAttempt = 0f;
		}
		return true;
	}

	public static bool Read(bool original, int main, int sub)
	{
		if (!Ready())
		{
			return original;
		}
		if (original)
		{
			Mark(main, sub);
		}
		if (!original)
		{
			return state.Contains(Key(main, sub));
		}
		return true;
	}

	public static void Mark(int main, int sub)
	{
		if (Ready())
		{
			string key = Key(main, sub);
			bool num = !state.Contains(key);
			state.Mark(key);
			if (num)
			{
				nextAttempt = System.Math.Min(nextAttempt, Time.realtimeSinceStartup + 0.5f);
			}
		}
	}

	public static void Tick()
	{
		if (!Ready())
		{
			state.SetScope("");
			busy = false;
			initialized = -1;
			return;
		}
		if (instance == null)
		{
			GameObject obj = new GameObject("SprkMissionHistory");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			instance = obj.AddComponent<SprkMissionHistory>();
		}
		if (initialized != state.Epoch)
		{
			initialized = state.Epoch;
			PlayerPrefsOpenHistoryData playerPrefsOpenHistoryData = SingletonAuto<PlayerPrefsOpenHistoryData>.instance;
			foreach (MissionCategoryData allDatum in GameRoot.Instance.MissionCategoryDataContainer.GetAllData())
			{
				if (MissionManagement.IsOpenSubCategory(allDatum) && playerPrefsOpenHistoryData.IsOpened_NewMark(allDatum.MainCategoryIndex, allDatum.SubCategoryIndex))
				{
					Mark(allDatum.MainCategoryIndex, allDatum.SubCategoryIndex);
				}
			}
		}
		if (!busy && Time.realtimeSinceStartup >= nextAttempt)
		{
			busy = true;
			instance.StartCoroutine(instance.Sync());
		}
	}

	private IEnumerator Sync()
	{
		int epoch = state.Epoch;
		string[] sent = state.Batch();
		bool success = false;
		WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
		string text = requester.Host.TrimEnd('/');
		string sessionKey = requester.SessionKey;
		try
		{
			string s = "SessionKey=" + UnityWebRequest.EscapeURL(sessionKey) + "&SeenCategories=" + UnityWebRequest.EscapeURL(Json.Serialize(sent));
			using UnityWebRequest request = new UnityWebRequest(text + "/mission/sync_seen_categories", "POST");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(s));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.timeout = 20;
			request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
			yield return request.SendWebRequest();
			if (!Ready() || state.Epoch != epoch || request.isNetworkError || request.isHttpError || !(Json.Deserialize(request.downloadHandler.text) is IDictionary dictionary) || Convert.ToString(dictionary["Result"]) != "Success" || !(dictionary["SeenMissionCategories"] is IList source))
			{
				yield break;
			}
			int revision = state.Revision;
			success = state.Merge(epoch, source.Cast<object>().Select(Convert.ToString), sent);
			if (success && revision != state.Revision)
			{
				MissionManagement missionManagement = UnityEngine.Object.FindObjectOfType<MissionManagement>();
				if (missionManagement != null)
				{
					missionManagement.RefreshQuestList();
				}
			}
		}
		finally
		{
			if (state.Epoch == epoch)
			{
				busy = false;
				nextAttempt = Time.realtimeSinceStartup + ((!success) ? 10f : ((state.PendingCount > 0) ? 0.5f : 60f));
			}
		}
	}
}
