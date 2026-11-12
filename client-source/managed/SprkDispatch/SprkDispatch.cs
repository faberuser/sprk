using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using LitJson;
using NGame2.NAccount;
using NGame2.NBattleSimulator;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NShared.CancelDispatch;
using NShared.GetDispatchList;
using NShared.RequestCompleteDispatch;
using NShared.StartDispatch;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Networking;

public sealed class SprkDispatch : MonoBehaviour
{
	private sealed class RewardDelivery
	{
		public EquipItemInfo[] Equipment;
	}

	[Serializable]
	public class Job
	{
		public int SlotIndex;

		public int ChapterIndex;

		public int DungeonIndex;

		public int Difficulty;

		public int RepeatCount;

		public int SimulationVersion;

		public string State;

		public string HeroIndices;

		public string SimulationJobId;

		public Prepared Saved = new Prepared();
	}

	public class Prepared
	{
		public readonly List<int> Outcomes = new List<int>();

		public readonly List<int> Times = new List<int>();
	}

	private static readonly ConditionalWeakTable<object, RewardDelivery> rewardDeliveries = new ConditionalWeakTable<object, RewardDelivery>();

	private static SprkDispatch instance;

	private bool busy;

	private float nextPoll;

	private bool wakePending;

	private string publishedSnapshot;

	private bool loggedReady;

	public string LastStatus;

	public string LastError;

	private BattleSimulationInstance active;

	private readonly Dictionary<string, Prepared> prepared = new Dictionary<string, Prepared>();

	private long owner;

	private string ownerSession;

	private static int lastFrame = -1;

	public static void CaptureRewards(string body, object response)
	{
		NShared.RequestCompleteDispatch.Response response2 = response as NShared.RequestCompleteDispatch.Response;
		NShared.CancelDispatch.Response response3 = response as NShared.CancelDispatch.Response;
		if ((response2 != null && response2.BaseResult == BaseResultType.Success && response2.Result == NShared.RequestCompleteDispatch.ResultType.Success) || (response3 != null && response3.BaseResult == BaseResultType.Success && response3.Result == NShared.CancelDispatch.ResultType.Success))
		{
			RewardResultInfo rewardResultInfo = JsonMarshaler.Parse<RewardResultInfo>(body);
			if (rewardResultInfo != null && rewardResultInfo.EquipItemResults != null && rewardResultInfo.EquipItemResults.Length != 0)
			{
				rewardDeliveries.Remove(response);
				rewardDeliveries.Add(response, new RewardDelivery
				{
					Equipment = rewardResultInfo.EquipItemResults
				});
			}
		}
	}

	public static void ApplyRewards(object response)
	{
		if (response != null && rewardDeliveries.TryGetValue(response, out var value))
		{
			Account().EquipItemManager.Apply(value.Equipment);
			rewardDeliveries.Remove(response);
		}
	}

	public static void Ensure()
	{
		if (!(instance != null))
		{
			GameObject obj = new GameObject("SprkDispatch");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			instance = obj.AddComponent<SprkDispatch>();
			Debug.Log("SPRK dispatch simulator initialized");
		}
	}

	public static void Started(NShared.StartDispatch.Response response)
	{
		if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.StartDispatch.ResultType.Success)
		{
			Ensure();
			instance.nextPoll = 0f;
			instance.wakePending = instance.busy;
		}
	}

	public static string HelpText(string text)
	{
		if (text == null)
		{
			return text;
		}
		return text.Replace("In Dispatch Battle, you cannot regain Stamina for the battles you have lost.", "In Dispatch Battle, only victories consume Stamina.").Replace("You cannot regain Stamina for battles you have lost.", "Defeated and unfinished battles consume no Stamina.");
	}

	public static void StartRejected(NShared.StartDispatch.Response response)
	{
		if (response == null)
		{
			MessagePopup.OpenError("Unable to start dispatch. Please try again.");
		}
		else if (response.BaseResult != BaseResultType.Success)
		{
			MessagePopup.OpenError(response.BaseResult);
		}
		else if (response.Result == NShared.StartDispatch.ResultType.Fail)
		{
			MessagePopup.Open("Dispatch", "Unable to start dispatch. Check the stage requirements, selected Heroes and available Stamina, then try again.");
		}
		else
		{
			MessagePopup.OpenError(response.Result);
		}
	}

	public static int Seed(int original, string uid)
	{
		if (uid == null || !uid.StartsWith("sprk-dispatch:"))
		{
			return original;
		}
		uint num = 2166136261u;
		foreach (char c in uid)
		{
			num = (num ^ c) * 16777619;
		}
		return (int)((num ^ (uint)original) & 0x7FFFFFFF);
	}

	private static AccountManager Account()
	{
		return GlobalAutoBehaviour<AccountManager>.instance;
	}

	private static bool Ready()
	{
		if (Account() != null && Account().LoginResponseComplete && Account().UserInfo != null && SingletonUpdater<WebServiceManager>.instance.Requester != null)
		{
			return !string.IsNullOrEmpty(SingletonUpdater<WebServiceManager>.instance.Requester.SessionKey);
		}
		return false;
	}

	private static string Host()
	{
		return SingletonUpdater<WebServiceManager>.instance.Requester.Host.TrimEnd('/');
	}

	private static string Session()
	{
		return SingletonUpdater<WebServiceManager>.instance.Requester.SessionKey;
	}

	public static void Tick()
	{
		if (lastFrame != Time.frameCount)
		{
			lastFrame = Time.frameCount;
			Ensure();
			instance.PollIfReady();
		}
	}

	private void PollIfReady()
	{
		if (busy || !Ready())
		{
			return;
		}
		if (owner != Account().UserInfo.AccountId || ownerSession != Session())
		{
			nextPoll = 0f;
		}
		if (!(Time.realtimeSinceStartup < nextPoll))
		{
			if (!loggedReady)
			{
				loggedReady = true;
				Debug.Log("SPRK dispatch simulator ready");
			}
			nextPoll = Time.realtimeSinceStartup + 3f;
			StartCoroutine(Poll());
		}
	}

	private void OnDestroy()
	{
		if (active != null)
		{
			active.Stop();
		}
		instance = null;
	}

	private void OnApplicationQuit()
	{
		if (active != null)
		{
			active.Stop();
		}
	}

	private IEnumerator Post(string path, Dictionary<string, string> fields, Action<string> done)
	{
		fields["SessionKey"] = Session();
		string s = string.Join("&", fields.Select((KeyValuePair<string, string> p) => UnityWebRequest.EscapeURL(p.Key) + "=" + UnityWebRequest.EscapeURL(p.Value)).ToArray());
		using UnityWebRequest web = new UnityWebRequest(Host() + "/" + path, "POST");
		web.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(s));
		web.downloadHandler = new DownloadHandlerBuffer();
		web.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		web.timeout = 20;
		yield return web.SendWebRequest();
		done((web.isNetworkError || web.isHttpError) ? null : web.downloadHandler.text);
	}

	private IEnumerator Poll()
	{
		busy = true;
		try
		{
			long accountId = Account().UserInfo.AccountId;
			if (owner != accountId || ownerSession != Session())
			{
				prepared.Clear();
				publishedSnapshot = null;
				owner = accountId;
				ownerSession = Session();
			}
			string pollSession = Session();
			string body = null;
			yield return Post("dispatch/get_dispatch_list", new Dictionary<string, string>(), delegate(string v)
			{
				body = v;
			});
			if (!Ready() || Account().UserInfo.AccountId != owner || Session() != pollSession)
			{
				yield break;
			}
			nextPoll = Time.realtimeSinceStartup + 15f;
			if (body == null)
			{
				Debug.LogWarning("SPRK dispatch list request failed");
				yield break;
			}
			JsonData jsonData = JsonMapper.ToObject(body);
			if (jsonData == null || jsonData["Result"].valueStr != "Success" || !jsonData["DispatchBattleInfos"].IsArray())
			{
				yield break;
			}
			JsonData jsonData2 = jsonData["DispatchBattleInfos"];
			Job[] array = new Job[jsonData2.Count];
			for (int num = 0; num < array.Length; num++)
			{
				JsonData fromArray = jsonData2.GetFromArray(num);
				array[num] = new Job
				{
					SlotIndex = fromArray["SlotIndex"].valueInt,
					ChapterIndex = fromArray["ChapterIndex"].valueInt,
					DungeonIndex = fromArray["DungeonIndex"].valueInt,
					Difficulty = fromArray["Difficulty"].valueInt,
					RepeatCount = fromArray["RepeatCount"].valueInt,
					SimulationVersion = fromArray["SimulationVersion"].valueInt,
					State = fromArray["State"].valueStr,
					HeroIndices = fromArray["HeroIndices"].valueStr,
					SimulationJobId = fromArray["SimulationJobId"].valueStr
				};
				JsonData jsonData3 = fromArray["SimulationResults"];
				if (jsonData3 != null && jsonData3.IsArray())
				{
					for (int num2 = 0; num2 < jsonData3.Count; num2++)
					{
						array[num].Saved.Outcomes.Add(jsonData3.GetFromArray(num2)["Outcome"].valueInt);
						array[num].Saved.Times.Add(jsonData3.GetFromArray(num2)["TimeMs"].valueInt);
					}
				}
			}
			jsonData.ReleaseAll();
			nextPoll = Time.realtimeSinceStartup + PollDelay(array);
			if (body != publishedSnapshot && (array.Length != 0 || publishedSnapshot != null))
			{
				ApplyList(body);
				publishedSnapshot = body;
			}
			LastStatus = "Received " + array.Length + " jobs";
			HashSet<string> hashSet = new HashSet<string>(from j in array.Where(NeedsPreparation)
				select j.SimulationJobId);
			string[] array2 = prepared.Keys.ToArray();
			foreach (string text in array2)
			{
				if (!hashSet.Contains(text))
				{
					prepared.Remove(text);
				}
			}
			Job[] array3 = array;
			foreach (Job job in array3)
			{
				if (!Ready() || Account().UserInfo.AccountId != owner || Session() != pollSession)
				{
					break;
				}
				if (NeedsPreparation(job))
				{
					yield return Prepare(job);
				}
			}
		}
		finally
		{
			SprkDispatch sprkDispatch = this;
			sprkDispatch.busy = false;
			if (sprkDispatch.wakePending)
			{
				sprkDispatch.wakePending = false;
				sprkDispatch.nextPoll = 0f;
			}
		}
	}

	public static bool NeedsPreparation(Job job)
	{
		if (job != null && job.SimulationVersion == 1 && (job.State == "Simulation" || job.State == "Battle"))
		{
			return job.Saved.Outcomes.Count < job.RepeatCount;
		}
		return false;
	}

	public static float PollDelay(Job[] jobs)
	{
		if (!jobs.Any((Job j) => j.State != "ReadyToComplete" && j.State != "Complete" && j.State != "Cancel" && j.State != "None"))
		{
			return 30f;
		}
		return 3f;
	}

	private static void ApplyList(string body)
	{
		NShared.GetDispatchList.Response response = JsonMarshaler.Parse<NShared.GetDispatchList.Response>(body);
		if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.GetDispatchList.ResultType.Success && Account().DispatchBattleInfoManager != null && response.DispatchBattleInfos != null)
		{
			Account().DispatchBattleInfoManager.Set((from info in response.DispatchBattleInfos
				where info != null
				select new WCL_DispatchBattleInfo().Copy(info)).ToArray());
		}
	}

	private IEnumerator Prepare(Job job)
	{
		string jobSession = Session();
		if (!prepared.TryGetValue(job.SimulationJobId, out var result) || result.Outcomes.Count < job.Saved.Outcomes.Count)
		{
			result = job.Saved;
			prepared[job.SimulationJobId] = result;
		}
		if (result.Outcomes.Count == job.Saved.Outcomes.Count && result.Outcomes.Count < job.RepeatCount)
		{
			JsonData jsonData = JsonMapper.ToObject(job.HeroIndices);
			int[] array = new int[jsonData.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = jsonData.GetFromArray(i).valueInt;
			}
			jsonData.ReleaseAll();
			if (array.Length == 0 || job.RepeatCount < 1 || job.RepeatCount > 200)
			{
				yield break;
			}
			int count = result.Outcomes.Count;
			string uid = "sprk-dispatch:" + job.SimulationJobId + ":" + count;
			try
			{
				active = BattleSimulationFacade.RunBattleSimulation(uid, array, job.ChapterIndex, job.DungeonIndex, job.Difficulty);
			}
			catch (Exception ex)
			{
				LastError = ex.ToString();
				Debug.LogWarning("SPRK dispatch preparation: " + ex.GetType().Name + ": " + ex.Message);
				nextPoll = Time.realtimeSinceStartup + 30f;
				yield break;
			}
			LastStatus = "Simulating run " + (count + 1) + " of " + job.RepeatCount;
			float deadline = Time.realtimeSinceStartup + 90f;
			while (!active.IsBattleEnd)
			{
				if (!Ready() || Account().UserInfo.AccountId != owner || Session() != jobSession || Time.realtimeSinceStartup > deadline)
				{
					BattleSimulationFacade.RemoveInstance(active);
					active = null;
					nextPoll = Time.realtimeSinceStartup + 30f;
					yield break;
				}
				yield return null;
			}
			BattleSimulationResultInfo battleSimulationResultInfo = active.GetBattleSimulationResultInfo();
			if (battleSimulationResultInfo == null || active.IsBattleStopped)
			{
				BattleSimulationFacade.RemoveInstance(active);
				active = null;
				nextPoll = Time.realtimeSinceStartup + 30f;
				yield break;
			}
			result.Outcomes.Add(battleSimulationResultInfo.IsWin ? 1 : 0);
			result.Times.Add((int)System.Math.Max(1000L, System.Math.Min(3600000L, battleSimulationResultInfo.RealTime)));
			BattleSimulationFacade.RemoveInstance(active);
			active = null;
		}
		if (!Ready() || Account().UserInfo.AccountId != owner || Session() != jobSession)
		{
			yield break;
		}
		string answer = null;
		yield return Post("dispatch/complete_calculate_dispatch_result", new Dictionary<string, string>
		{
			{
				"SlotIndex",
				job.SlotIndex.ToString()
			},
			{ "SimulationJobId", job.SimulationJobId },
			{ "Progressive", "true" },
			{
				"Results",
				"[" + string.Join(",", result.Outcomes.Select((int v) => v.ToString()).ToArray()) + "]"
			},
			{
				"TimesMs",
				"[" + string.Join(",", result.Times.Select((int v) => v.ToString()).ToArray()) + "]"
			}
		}, delegate(string v)
		{
			answer = v;
		});
		if (Ready() && Account().UserInfo.AccountId == owner && !(Session() != jobSession) && answer != null)
		{
			JsonData jsonData2 = JsonMapper.ToObject(answer);
			if (jsonData2 != null && jsonData2["Result"].valueStr == "Success")
			{
				LastStatus = "Prepared " + result.Outcomes.Count + " of " + job.RepeatCount + " battles";
				publishedSnapshot = null;
			}
			else
			{
				prepared.Remove(job.SimulationJobId);
				nextPoll = Time.realtimeSinceStartup + 3f;
			}
			jsonData2?.ReleaseAll();
		}
	}
}
