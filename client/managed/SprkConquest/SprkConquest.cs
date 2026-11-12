using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MiniJSON;
using NGame2.NBattleField;
using NGame2.NParty;
using NGame2.NUI.NWindow.NBattle;
using NGameAsset;
using NShared;
using NShared.NCampaign;
using NShared.NJit;
using NShared.NParty;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkConquest : MonoBehaviour
{
	private class Run : IBattleLogListener
	{
		public SprkConquest owner;

		public string id;

		public CampaignUpdater updater;

		public long hp;

		public int boss;

		public bool ended;

		public double budget;

		public Dictionary<int, long> heroes = new Dictionary<int, long>();

		public HashSet<long> players = new HashSet<long>();

		public bool arena;

		public bool dispatch;

		public HashSet<long> withdrawn = new HashSet<long>();

		public List<object> logs = new List<object>();

		public Dictionary<string, int> defeated = new Dictionary<string, int>();

		private readonly HashSet<string> defeatedKeys = new HashSet<string>();

		private long lastClockMs = -1L;

		public void CaptureDefeats()
		{
			if (arena)
			{
				return;
			}
			foreach (ResultCreatureInfo creatureInfo in BattleInstanceIntegrityChecker.GetCreatureInfoList(updater.BattleInstance))
			{
				if (creatureInfo.TeamId == 1 && creatureInfo.Hp <= 0 && creatureInfo.MaxHp > 0)
				{
					string item = updater.WaveIndex + ":" + creatureInfo.Key;
					if (defeatedKeys.Add(item))
					{
						string key = creatureInfo.Index.ToString();
						defeated[key] = (defeated.TryGetValue(key, out var value) ? value : 0) + 1;
					}
				}
			}
		}

		public void OnProcessBattleLog(long time, NShared.BattleLog log)
		{
			if (log.Type == BattleLogType.InitialDungeonBattleLog && updater.BattleInstance.CheckEachWaveEachTeam)
			{
				InitializeDungeonBattleLog initializeDungeonBattleLog = JM_NShared_InitializeDungeonBattleLog.Parse(log.Data);
				BattleCreatureCreateInfo[] array = initializeDungeonBattleLog.CreateInfos ?? new BattleCreatureCreateInfo[0];
				foreach (BattleCreatureCreateInfo battleCreatureCreateInfo in array)
				{
					updater.BattleInstance.BattleLogContainer.AddLog(new CreatureCreateBattleLog
					{
						PlayerId = (heroes.TryGetValue(battleCreatureCreateInfo.HeroInfo.HeroIndex, out var value) ? value : initializeDungeonBattleLog.PlayerId),
						CreateInfo = battleCreatureCreateInfo
					});
				}
				initializeDungeonBattleLog.CreateInfos = new BattleCreatureCreateInfo[0];
				log.BaseBattleLog.Data = JsonMarshaler.EncodeJsonString(initializeDungeonBattleLog);
			}
			if (log.Type == BattleLogType.WaveReady && dispatch)
			{
				foreach (long player in players)
				{
					updater.BattleInstance.BattleLogContainer.AddLog(new CreatureSetAiBattleLog
					{
						PlayerId = player,
						SetAi = true
					});
				}
			}
			if (log.Type == BattleLogType.WaveReady && !arena && !dispatch)
			{
				IDictionary dictionary = Json.Deserialize(log.Data) as IDictionary;
				WaveReadyBattleLog waveReadyBattleLog = JM_NShared_WaveReadyBattleLog.Parse(dictionary);
				List<object> list = new List<object>();
				BattleCreatureCreateInfo[] array = waveReadyBattleLog.MonsterCreateInfos ?? new BattleCreatureCreateInfo[0];
				foreach (BattleCreatureCreateInfo battleCreatureCreateInfo2 in array)
				{
					if (battleCreatureCreateInfo2.HeroInfo.HeroIndex == boss)
					{
						list.Add(JM_NShared_ResultCreatureInfo.EncodeJsonObject(new ResultCreatureInfo
						{
							Index = boss,
							TeamId = 1,
							Key = ResultCreatureInfo.GetKey(boss, battleCreatureCreateInfo2.WaveSlotIndex, 1),
							Hp = hp,
							MaxHp = battleCreatureCreateInfo2.StatData.GetStat(StatType.MaxHp),
							Mp = 0,
							MaxMp = (int)battleCreatureCreateInfo2.StatData.GetStat(StatType.MaxMp)
						}));
					}
				}
				dictionary["ConquestInitialCreatures"] = list;
				log.BaseBattleLog.Data = Json.Serialize(dictionary);
			}
			if (!dispatch)
			{
				logs.Add(JM_NShared_BaseBattleLog.EncodeJsonObject(log.BaseBattleLog));
			}
			if (log.Type == BattleLogType.WaveReady || log.Type == BattleLogType.WaveEnd)
			{
				if (log.Type == BattleLogType.WaveEnd)
				{
					CaptureDefeats();
				}
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>
				{
					{ "WaveIndex", updater.WaveIndex },
					{ "TotalWaveIndex", updater.TotalWaveIndex }
				};
				if (log.Type == BattleLogType.WaveEnd)
				{
					dictionary2["Winner"] = JM_NShared_WaveEndBattleLog.Parse(log.Data).winner.ToString();
				}
				Flush();
				owner.Send("WorkerPacket", new Dictionary<string, object>
				{
					{ "RunId", id },
					{
						"Name",
						(log.Type == BattleLogType.WaveReady) ? "WaveReadyNot" : "WaveEndNot"
					},
					{ "Body", dictionary2 }
				});
			}
		}

		public void Flush()
		{
			if (!dispatch)
			{
				long currentTimeMs = updater.BattleInstance.GlobalTimer.GetCurrentTimeMs();
				if (logs.Count != 0 || currentTimeMs != lastClockMs)
				{
					lastClockMs = currentTimeMs;
					owner.Send("WorkerPacket", new Dictionary<string, object>
					{
						{ "RunId", id },
						{ "Name", "BattleLogNot" },
						{
							"Body",
							new Dictionary<string, object>
							{
								{
									"BattleLogs",
									logs.ToArray()
								},
								{ "ServerTimeMs", currentTimeMs }
							}
						}
					});
					logs.Clear();
				}
			}
		}
	}

	private static SprkConquest instance;

	private int lastFrame = -1;

	private static readonly Dictionary<WaveReadyBattleLog, ResultCreatureInfo[]> seeds = new Dictionary<WaveReadyBattleLog, ResultCreatureInfo[]>();

	private static readonly bool trace = Environment.GetEnvironmentVariable("SPRK_BATTLE_TRACE") == "1";

	private static readonly object traceGate = new object();

	private static volatile SprkReplayClock replayClock;

	private readonly Dictionary<string, Run> runs = new Dictionary<string, Run>();

	private volatile SprkBattleLink link;

	private float retryAt;

	private float lastHeartbeat;

	private long lastStep;

	private int firstRun;

	private int connecting;

	public static bool FilterClock(ref long time)
	{
		return replayClock?.Accept(ref time) ?? true;
	}

	public static void Trace(string message)
	{
		if (!trace)
		{
			return;
		}
		try
		{
			string text = Environment.GetEnvironmentVariable("SPRK_BATTLE_TRACE_FILE");
			if (string.IsNullOrEmpty(text))
			{
				text = Path.Combine(Application.persistentDataPath, "sprk-battle.log");
			}
			lock (traceGate)
			{
				File.AppendAllText(text, DateTime.UtcNow.ToString("O") + " " + message + Environment.NewLine);
			}
		}
		catch
		{
		}
	}

	public static bool ArenaAutoPlayable(BattleType type, ref bool auto, ref bool fast)
	{
		if (type != BattleType.Arena && type != BattleType.ArenaBanPick)
		{
			return false;
		}
		auto = true;
		fast = false;
		return true;
	}

	public static bool ArenaAuto(bool original, object control)
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return original;
		}
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (battleCache == null || battleCache.ChapterIndex != 1000 || (battleCache.BattleType != BattleType.Arena && battleCache.BattleType != BattleType.ArenaBanPick))
		{
			return original;
		}
		SystemButtonAutoSimple systemButtonAutoSimple = control as SystemButtonAutoSimple;
		if (systemButtonAutoSimple != null)
		{
			return systemButtonAutoSimple.IsAuto();
		}
		SystemButtonAuto systemButtonAuto = control as SystemButtonAuto;
		if (!(systemButtonAuto == null))
		{
			return systemButtonAuto.IsAuto();
		}
		return original;
	}

	public static void LeaveSoloConquestOnLobbyReturn()
	{
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance)
		{
			PartyManager partyManager = GlobalAutoBehaviour<PartyManager>.instance;
			if (IsSoloConquestParty(partyManager.PartyInfo))
			{
				partyManager.SendLeaveParty();
			}
		}
	}

	public static bool IsSoloConquestParty(PartyInfo info)
	{
		if (info != null && info.RaidIndex == 90001 && info.RaidLevel >= 101)
		{
			return info.RaidLevel <= 103;
		}
		return false;
	}

	public static Dictionary<string, object> Login(Dictionary<string, object> value)
	{
		if (!int.TryParse(Environment.GetEnvironmentVariable("SPRK_ARENA_REPLAY_BUFFER_MS"), out var result))
		{
			result = 100;
		}
		bool flag = value.ContainsKey("MatchUid") && Convert.ToInt64(value["MatchUid"]) > 0;
		bool flag2 = value.ContainsKey("MasterId") && Convert.ToInt64(value["MasterId"]) > 0;
		replayClock = ((flag || flag2) ? new SprkReplayClock(flag ? result : 0) : null);
		WebServiceManager webServiceManager = SingletonUpdater<WebServiceManager>.instance;
		if (webServiceManager != null && webServiceManager.Requester != null)
		{
			value["SessionKey"] = webServiceManager.Requester.SessionKey;
		}
		return value;
	}

	public static Dictionary<string, object> ReplayStatus(Dictionary<string, object> value)
	{
		BattleFacade.BattleCacheData battleCacheData = (SingletonAuto<BattleFacade>.isValidInstance ? SingletonAuto<BattleFacade>.instance.GetBattleCache() : null);
		value["BattleStopped"] = battleCacheData?.IsBattleEnded ?? false;
		value["ReplayWaveIndex"] = battleCacheData?.WaveIndex ?? 0;
		return value;
	}

	public static BattleLogNot ReadClock(BattleLogNot value, IDictionary data)
	{
		if (value != null && data.Contains("ServerTimeMs") && long.TryParse(Convert.ToString(data["ServerTimeMs"]), out var result) && result >= 0 && SingletonAuto<BattleFacade>.isValidInstance && SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			if (trace)
			{
				BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
				if (battleCache != null)
				{
					Trace("clock serverMs=" + result + " replayMs=" + battleCache.GlobalCurrentTimeMs + " leadMs=" + (battleCache.GlobalCurrentTimeMs - result));
				}
			}
			SingletonAuto<BattleFacade>.instance.SetServerTimeMs(result);
		}
		return value;
	}

	public static WaveReadyBattleLog ReadSeed(WaveReadyBattleLog value, IDictionary data)
	{
		if (value != null && value.ChapterIndex == 9900 && data.Contains("ConquestInitialCreatures") && data["ConquestInitialCreatures"] is IList source)
		{
			seeds[value] = source.Cast<IDictionary>().Select(JM_NShared_ResultCreatureInfo.Parse).ToArray();
		}
		return value;
	}

	public static void ApplySeed(WaveReadyBattleLog value, BattleInstance battle)
	{
		if (seeds.TryGetValue(value, out var value2))
		{
			battle.SetInitialCreatureInfo(value2);
			seeds.Remove(value);
		}
	}

	public static List<ResultCreatureInfo> ScoreBoss(List<ResultCreatureInfo> infos, BattleInstance battle)
	{
		if (battle == null || battle.ChapterIndex != 9900 || (battle.DungeonIndex != 3 && battle.DungeonIndex != 103))
		{
			return infos;
		}
		foreach (ResultCreatureInfo info in infos)
		{
			if (info.TeamId == 1 && info.Index == 8801)
			{
				BattleCreature creatureByKey = battle.CreatureContainer.GetCreatureByKey(info.Key);
				if (creatureByKey != null)
				{
					info.GivedDamage = battle.Snapshot.StatisticsManager.TotalDamagedMeter(creatureByKey.Uid);
				}
			}
		}
		return infos;
	}

	public static void Tick()
	{
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_CONQUEST_WORKER_KEY")))
		{
			UnityEngine.Debug.unityLogger.logEnabled = true;
			if (instance == null)
			{
				GameObject obj = new GameObject("SprkConquestService");
				UnityEngine.Object.DontDestroyOnLoad(obj);
				instance = obj.AddComponent<SprkConquest>();
				Application.runInBackground = true;
				Application.targetFrameRate = 60;
				UnityEngine.Debug.Log("SPRK conquest service initialized");
			}
			instance.Step();
		}
	}

	private void Update()
	{
		Step();
	}

	private void Connect()
	{
		if (Interlocked.CompareExchange(ref connecting, 1, 0) != 0)
		{
			return;
		}
		retryAt = Time.realtimeSinceStartup + 5f;
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		if (GameRoot.Instance.JsonLoader == null)
		{
			UnityEngine.Debug.Log("SPRK native worker loading table assets");
			SingletonAuto<TableDataManager>.instance.Load();
			connecting = 0;
			return;
		}
		CreatureDataContainer creatureDataContainer = GameRoot.Instance.CreatureDataContainer;
		if (creatureDataContainer == null || !creatureDataContainer.GetAllData().Any())
		{
			UnityEngine.Debug.Log("SPRK native worker waiting for creature table");
			connecting = 0;
			return;
		}
		UnityEngine.Debug.Log("SPRK native worker connecting");
		Thread thread = new Thread((ThreadStart)delegate
		{
			TcpClient tcpClient = null;
			try
			{
				TcpClient tcpClient2 = (tcpClient = new TcpClient());
				tcpClient2.NoDelay = true;
				if (!int.TryParse(Environment.GetEnvironmentVariable("SPRK_CONQUEST_WORKER_PORT"), out var result))
				{
					result = 9002;
				}
				tcpClient2.Connect(Environment.GetEnvironmentVariable("SPRK_CONQUEST_WORKER_HOST") ?? "127.0.0.1", result);
				SprkBattleLink sprkBattleLink = new SprkBattleLink(tcpClient2, Json.Serialize, delegate(string message)
				{
					UnityEngine.Debug.LogWarning("SPRK worker transport: " + message);
				});
				sprkBattleLink.Send("WorkerLogin", new Dictionary<string, object>
				{
					{
						"Key",
						Environment.GetEnvironmentVariable("SPRK_CONQUEST_WORKER_KEY")
					},
					{ "ArenaVersion", 1 },
					{ "ChallengeRaidVersion", 1 },
					{ "DispatchVersion", 1 }
				});
				link = sprkBattleLink;
				sprkBattleLink.ReceiveLoop();
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogWarning("SPRK conquest service connection: " + ex.Message);
			}
			finally
			{
				tcpClient?.Close();
				Interlocked.Exchange(ref connecting, 0);
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void Send(string name, object value)
	{
		link?.Send(name, value);
	}

	private void Step()
	{
		UnityEngine.Debug.unityLogger.logEnabled = true;
		if (lastFrame == Time.frameCount)
		{
			return;
		}
		lastFrame = Time.frameCount;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		long timestamp = Stopwatch.GetTimestamp();
		if (link == null || !link.Connected)
		{
			if (runs.Count > 0)
			{
				runs.Clear();
				UnityEngine.Debug.LogWarning("SPRK conquest service disconnected; active runs cancelled");
			}
			if (realtimeSinceStartup >= retryAt)
			{
				Connect();
			}
			lastStep = timestamp;
			return;
		}
		if (realtimeSinceStartup - lastHeartbeat >= 5f)
		{
			lastHeartbeat = realtimeSinceStartup;
			Send("WorkerPing", new Dictionary<string, object>());
		}
		try
		{
			for (int i = 0; i < 100; i++)
			{
				if (!link.TryReceive(out var line))
				{
					break;
				}
				string[] array = line.Split(new char[1] { ' ' }, 2);
				IDictionary dictionary = Json.Deserialize(Encoding.UTF8.GetString(Convert.FromBase64String(array[1]))) as IDictionary;
				if (array[0] == "WorkerLoginRes")
				{
					UnityEngine.Debug.Log("SPRK conquest service authenticated");
					continue;
				}
				string text = Convert.ToString(dictionary["RunId"]);
				if (array[0] == "WorkerPrepare")
				{
					Prepare(text, dictionary);
				}
				else
				{
					if (!runs.TryGetValue(text, out var value))
					{
						continue;
					}
					if (array[0] == "WorkerStart")
					{
						value.updater.StartBattle();
					}
					else if (array[0] == "WorkerResume")
					{
						value.updater.SetWavePauseFalse();
					}
					else if (array[0] == "WorkerCancel")
					{
						runs.Remove(text);
					}
					else if (array[0] == "WorkerCommand")
					{
						Command(value, dictionary);
					}
					else if (array[0] == "WorkerWithdraw")
					{
						long num = Convert.ToInt64(dictionary["AccountId"]);
						value.withdrawn.Add(num);
						value.updater.BattleInstance.BattleLogContainer.AddLog(new CreatureSetAiBattleLog
						{
							PlayerId = num,
							SetAi = true
						});
						if (value.players.All(value.withdrawn.Contains))
						{
							value.updater.BattleInstance.BattleLogContainer.AddLog(new WaveWithdrawBattleLog());
						}
					}
				}
			}
			double num2 = (double)(timestamp - lastStep) * 1000.0 / (double)Stopwatch.Frequency;
			lastStep = timestamp;
			Run[] array2 = runs.Values.ToArray();
			Run[] array3 = array2;
			foreach (Run run in array3)
			{
				if (!run.dispatch)
				{
					run.budget += num2;
				}
			}
			for (int k = 0; k < 5; k++)
			{
				for (int l = 0; l < array2.Length; l++)
				{
					Run run2 = array2[(firstRun + l) % array2.Length];
					if (!run2.dispatch && !run2.ended && !(run2.budget < 100.0))
					{
						run2.budget -= 100.0;
						run2.updater.Update();
						run2.updater.BattleInstance.UpdateTick(snapshotEnabled: false);
						SnapshotObjectPoolContainer.FlushPendingRelease();
						run2.Flush();
						if (run2.updater.IsBattleEnd())
						{
							Finish(run2);
						}
					}
				}
			}
			Run[] array4 = array2.Where((Run r) => r.dispatch && !r.ended).ToArray();
			long sliceStart = Stopwatch.GetTimestamp();
			SprkDispatchPacing.Pump(array4, delegate(Run run3)
			{
				if (run3.ended)
				{
					return false;
				}
				run3.updater.Update();
				run3.updater.BattleInstance.UpdateTick(snapshotEnabled: false);
				SnapshotObjectPoolContainer.FlushPendingRelease();
				if (run3.updater.IsBattleEnd())
				{
					Finish(run3);
				}
				return true;
			}, () => (double)(Stopwatch.GetTimestamp() - sliceStart) * 1000.0 / (double)Stopwatch.Frequency < 8.0, firstRun);
			if (array2.Length != 0)
			{
				firstRun = (firstRun + 1) % array2.Length;
			}
			if (trace && num2 > 150.0)
			{
				Trace("worker frameGapMs=" + num2 + " runs=" + array2.Length + " maxDebtMs=" + ((array2.Length == 0) ? 0.0 : array2.Max((Run r) => r.budget)));
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("SPRK conquest service: " + ex);
			if (link != null)
			{
				link.Close();
			}
		}
	}

	private void Prepare(string id, IDictionary data)
	{
		if (runs.ContainsKey(id))
		{
			return;
		}
		GameRoot.Instance.InitForClient();
		GameRoot.Instance.BattleLogFactory.SetSerializer(JsonMarshaler.EncodeJsonString);
		GameRoot.Instance.BattleLogFactory.SetDeserializer(JsonMarshaler.Parse);
		GameRoot.Instance.OnDataContainerLoaded(editorVerify: false);
		BattleSimulationInfo info = JM_NShared_BattleSimulationInfo.Parse(Json.Serialize(data["Simulation"]));
		BattleInstanceBuilder battleInstanceBuilder = new BattleInstanceBuilder();
		battleInstanceBuilder.SetCampaignDungeon(info.ChapterIndex, info.DungeonIndex, info.DungeonDifficulty);
		battleInstanceBuilder.EnableSnapshot(enable: false);
		IList list = (data.Contains("AffixIndices") ? (data["AffixIndices"] as IList) : null);
		if (list != null)
		{
			battleInstanceBuilder.SetExtraData(new BattleInstanceExtraData
			{
				AffixIndices = list.Cast<object>().Select(Convert.ToInt32).ToArray()
			});
		}
		List<IBattleLogRuntime> list2 = new List<IBattleLogRuntime>();
		Run run = new Run
		{
			owner = this,
			id = id,
			hp = Convert.ToInt64(data["BossHp"]),
			boss = Convert.ToInt32(data["BossIndex"]),
			arena = (data.Contains("ArenaMatch") && Convert.ToBoolean(data["ArenaMatch"])),
			dispatch = (data.Contains("Dispatch") && Convert.ToBoolean(data["Dispatch"]))
		};
		CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(info.ChapterIndex, info.DungeonIndex);
		bool flag = data2 != null && (data2.BattleType == BattleType.ArcdimRaid || data2.BattleType == BattleType.ArcdimHardRaid);
		List<BattleCreatureCreateInfo>[] array = new List<BattleCreatureCreateInfo>[2]
		{
			new List<BattleCreatureCreateInfo>(),
			new List<BattleCreatureCreateInfo>()
		};
		MatchInfo matchInfo = (run.arena ? JM_NShared_MatchInfo.Parse(Json.Serialize(data["MatchInfo"])) : null);
		BattleSimulationAccountInfo[] accountInfos = info.AccountInfos;
		foreach (BattleSimulationAccountInfo account in accountInfos)
		{
			run.players.Add(account.UserInfo.AccountId);
			MatchedAccountInfo matchedAccountInfo = matchInfo?.AccountInfos.First((MatchedAccountInfo a) => a.AccountId == account.UserInfo.AccountId);
			List<BattleCreatureCreateInfo> list3 = new List<BattleCreatureCreateInfo>();
			List<BattleCreatureCreateInfo> list4 = new List<BattleCreatureCreateInfo>();
			foreach (CachedHeroInfo value in account.HeroInfos.Values)
			{
				value.Validate();
				run.heroes[value.HeroIndex] = account.UserInfo.AccountId;
				bool flag2 = account.GroupHeroInfos != null && account.GroupHeroInfos.ContainsKey(value.HeroIndex);
				bool flag3 = flag2 && info.ChapterIndex == 9900 && info.DungeonIndex >= 101 && info.DungeonIndex <= 103;
				BattleCreatureCreateInfo battleCreatureCreateInfo = BattleCreatureCreateInfo.CreateMyCreatureCreateInfo(value, matchedAccountInfo != null && value.HeroIndex == matchedAccountInfo.LeaderHeroIndex, flag3 || run.dispatch, new BattleCreatureCreateInfo.BuffSkillInfo
				{
					GuildSkillDataBases = account.GuildSkills,
					AccountBuffDataBases = account.AccountBuffs,
					ClassBuffDataBasess = account.ClassBuffDataBases
				}, new BattleCreatureCreateInfo.BuffStatInfo
				{
					ExtraStatDatas = account.ExtraStatDatas,
					PetStatDataBases = account.PetStatDataBases
				}, (!flag && flag2 && !flag3) ? 1 : 0);
				if (run.dispatch && (!battleCreatureCreateInfo.IsAi || !battleCreatureCreateInfo.IsSmartAi))
				{
					throw new InvalidOperationException("Dispatch heroes must use native simulation AI");
				}
				if (flag)
				{
					array[flag2 ? 1u : 0u].Add(battleCreatureCreateInfo);
				}
				if (flag && flag2)
				{
					list4.Add(battleCreatureCreateInfo);
				}
				else
				{
					list3.Add(battleCreatureCreateInfo);
				}
			}
			list2.Add(new PlayerEnterBattleLog
			{
				PlayerCreateInfo = new BattlePlayerCreateInfo
				{
					Id = account.UserInfo.AccountId,
					TeamType = (matchedAccountInfo?.TeamType ?? TeamType.Left)
				},
				CreatureCreateInfos = list3.ToArray(),
				BattleCheckCreatureCreateInfos = list4.ToArray()
			});
		}
		battleInstanceBuilder.SetEnterPlayerBattleLogList(list2);
		run.updater = battleInstanceBuilder.BuildUpdater<BattleInstance>(-1, Convert.ToInt32(data["Seed"]));
		if (flag)
		{
			List<BattleCreatureCreateInfo>[] array2 = array;
			foreach (List<BattleCreatureCreateInfo> infoList in array2)
			{
				run.updater.AddEachWaveEachTeamBattleCreatureCreateInfo(infoList);
			}
			run.updater.SetWavePauseCallback(delegate
			{
				run.Flush();
				Send("WorkerPacket", new Dictionary<string, object>
				{
					{ "RunId", id },
					{ "Name", "SetNextTeamNot" },
					{
						"Body",
						new Dictionary<string, object>
						{
							{ "ChatperIndex", info.ChapterIndex },
							{ "DungeonIndex", info.DungeonIndex },
							{
								"DeckIndex",
								run.updater.WaveIndex - 1
							}
						}
					}
				});
			});
		}
		run.updater.BattleInstance.BattleLogContainer.SetListener(run);
		run.updater.BattleInstance.SetDeltaTimeReductionRatio(1);
		runs[id] = run;
		UnityEngine.Debug.Log("SPRK conquest service prepared " + id + " with " + run.heroes.Count + " heroes");
	}

	private void Command(Run run, IDictionary data)
	{
		string text = Convert.ToString(data["Name"]);
		IDictionary dictionary = data["Body"] as IDictionary;
		int creatureUid = Convert.ToInt32(dictionary["CreatureUid"]);
		long num = Convert.ToInt64(data["AccountId"]);
		BattleCreature creature = run.updater.BattleInstance.CreatureContainer.GetCreature(creatureUid);
		if (creature != null && creature.PlayerId == num)
		{
			if (trace)
			{
				Trace("command run=" + run.id + " name=" + text + " request=" + Convert.ToString(dictionary["RequestId"]) + " simulationMs=" + run.updater.BattleInstance.GlobalTimer.GetCurrentTimeMs() + " relayAndQueueMs=" + (data.Contains("QueuedAtUnixMs") ? ((DateTime.UtcNow.Ticks - 621355968000000000L) / 10000 - Convert.ToInt64(data["QueuedAtUnixMs"])) : (-1)));
			}
			IBattleLogRuntime battleLogRuntime = null;
			if (text == "ReserveOnSkillReq")
			{
				battleLogRuntime = JM_NShared_CreatureReserveOnSkillBattleLog.Parse(dictionary);
			}
			if (text == "ReserveOffSkillReq")
			{
				battleLogRuntime = JM_NShared_CreatureReserveOffSkillBattleLog.Parse(dictionary);
			}
			if (text == "FireNextSkillReq")
			{
				battleLogRuntime = JM_NShared_CreatureFireNextSkillBattleLog.Parse(dictionary);
			}
			if (text == "SetManualTargetReq")
			{
				battleLogRuntime = JM_NShared_CreatureManualTargetBattleLog.Parse(dictionary);
			}
			if (text == "SetManualMoveReq")
			{
				battleLogRuntime = JM_NShared_CreatureManualMoveBattleLog.Parse(dictionary);
			}
			if (battleLogRuntime != null)
			{
				run.updater.BattleInstance.BattleLogContainer.AddLog(battleLogRuntime);
			}
		}
	}

	private void Finish(Run run)
	{
		run.CaptureDefeats();
		run.ended = true;
		BattleInstance battleInstance = run.updater.BattleInstance;
		List<ResultCreatureInfo> creatureInfoList = BattleInstanceIntegrityChecker.GetCreatureInfoList(battleInstance);
		List<object> list = new List<object>();
		foreach (ResultCreatureInfo item in creatureInfoList)
		{
			Dictionary<string, object> dictionary = JM_NShared_ResultCreatureInfo.EncodeJsonObject(item);
			BattleCreature creatureByKey = battleInstance.CreatureContainer.GetCreatureByKey(item.Key);
			dictionary["AccountId"] = creatureByKey?.PlayerId ?? 0;
			dictionary["DealtDamage"] = ((creatureByKey == null) ? 0 : battleInstance.Snapshot.StatisticsManager.TotalDamageMeter(creatureByKey.Uid));
			list.Add(dictionary);
		}
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		foreach (long player in run.players)
		{
			dictionary2[player.ToString()] = battleInstance.Snapshot.StatisticsManager.GetTotalDamageOfAccount(player);
		}
		Send("WorkerResult", new Dictionary<string, object>
		{
			{ "RunId", run.id },
			{
				"Win",
				run.updater.GetWinner() == TeamType.Left
			},
			{
				"ReplayLogs",
				run.arena ? ReplayUtil.CreateBattleLogString(battleInstance, (object value) => JsonMarshaler.EncodeJsonString(value), compress: true) : ""
			},
			{
				"Winner",
				run.updater.GetWinner().ToString()
			},
			{ "Creatures", list },
			{ "DefeatedCreatures", run.defeated },
			{ "AccountDamage", dictionary2 },
			{
				"TimeMs",
				battleInstance.MainTimer.GetCurrentTimeMs()
			},
			{
				"MaxWaveIndex",
				run.updater.MaxWaveIndex
			}
		});
		runs.Remove(run.id);
	}

	private void OnApplicationQuit()
	{
		if (link != null)
		{
			link.Close();
		}
	}
}
