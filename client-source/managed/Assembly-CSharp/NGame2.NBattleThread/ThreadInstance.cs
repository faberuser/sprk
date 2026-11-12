using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NGame2.NAccount;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleThread.NEventService;
using NGame2.NBattleThread.NSystemThread;
using NGame2.NContents;
using NGame2.NUI.NManager;
using NGame2.NUtil;
using NShared;
using NShared.NCampaign;
using NShared.NCampaign.Karma;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleThread;

public class ThreadInstance : IBattleEventServiceContext
{
	public static LocalStorageBool UsingThreadFake = new LocalStorageBool(i_default: false, typeof(ThreadInstance).FullName + "UsingThreadFake");

	public static LocalStorageBool SkipNodeTree = new LocalStorageBool(i_default: false, typeof(ThreadInstance).FullName + "SkipNodeTree");

	private EventRouter _eventRouter;

	private EventRaiser _eventRaiser;

	private object _threadContextQueueSync = new object();

	private Queue<ThreadContext> _threadContextQueue = new Queue<ThreadContext>();

	private ThreadContext _workerThreadContext;

	private SyncThreadQueue<IThreadAction> _threadActionQueue = new SyncThreadQueue<IThreadAction>();

	internal CampaignUpdater _campaignUpdater;

	private EclipseOfflineModeUpdater _eclipseUpdater;

	private KarmaUpdater _karmaUpdater;

	private bool _campaignWaveReady;

	private bool _campaignWaveEnd;

	private bool _campaignWavePause;

	internal BattleInstance _battleInstance;

	private int _offlineElapsedTick;

	private ISystemThread _systemThread;

	public IThreadSendQueue<IThreadAction> ThreadActionSender => _threadActionQueue;

	public ThreadContext WorkerThreadContext
	{
		get
		{
			if (_workerThreadContext == null)
			{
				_workerThreadContext = new ThreadContext();
			}
			return _workerThreadContext;
		}
	}

	BattleInstance IBattleEventServiceContext.BattleInstance
	{
		get
		{
			return _battleInstance;
		}
		set
		{
			_battleInstance = value;
		}
	}

	public bool Open(int chapterIndex, int dungeonIndex, int difficulty, bool isScenario, bool isOnlineMode, BattleInstanceExtraData extraData, int onlineGameSpeedRatio)
	{
		_eventRouter = new EventRouter();
		_eventRaiser = new EventRaiser(_eventRouter);
		BattleInstanceBuilder battleInstanceBuilder = new BattleInstanceBuilder();
		battleInstanceBuilder.SetCampaignDungeon(chapterIndex, dungeonIndex, difficulty, isScenario);
		battleInstanceBuilder.SetGameSpeedRatio(onlineGameSpeedRatio);
		battleInstanceBuilder.SetBattleEventContext(this);
		battleInstanceBuilder.SetBattleEventRaiser(_eventRaiser);
		battleInstanceBuilder.EnableSnapshot(isOnlineMode);
		battleInstanceBuilder.SetExtraData(extraData);
		if (ClientValueManager.IsEnableTraceLogToFile)
		{
			battleInstanceBuilder.EnableTraceLogToFile(isEnable: true);
			if (AccountManager.IsLogined())
			{
				long accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
				battleInstanceBuilder.SetTraceBattleEventAccountId(accountId);
			}
		}
		_battleInstance = battleInstanceBuilder.Build<BattleInstance>();
		bool enhanceMode = IsEnhanceModeContent();
		_systemThread = new ThreadReal(this, isOnlineMode, enhanceMode);
		return true;
	}

	public void CreateEclipseUpdate(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario, List<IBattleLogRuntime> battleLogList, int randomSeedByMonth, int[] projectileGroupIndicesFromServer, EclipseOfflineModeUpdater.IFuncEclipse FuncEclipse, IDataProvider dataProvider)
	{
		if (_battleInstance != null && _campaignUpdater == null && dataProvider != null && dataProvider.GetBattleType() == BattleType.Eclipse)
		{
			_campaignUpdater = new EclipseOfflineModeUpdater(_battleInstance, chapterIndex, dungeonIndex, difficulty, isScenario, battleLogList, randomSeedByMonth, FuncEclipse, projectileGroupIndicesFromServer);
			EclipseOfflineModeUpdater eclipseOfflineModeUpdater = _campaignUpdater as EclipseOfflineModeUpdater;
			_campaignUpdater.SetWaveReadyCallback(delegate
			{
				_campaignWaveReady = true;
			});
			_campaignUpdater.SetWaveEndCallback(delegate
			{
				_campaignWaveEnd = true;
			});
			_campaignUpdater.SetWavePauseCallback(delegate(TeamType winner)
			{
				_campaignWavePause = true;
				eclipseOfflineModeUpdater.WavePause(winner);
			});
			_eclipseUpdater = _campaignUpdater as EclipseOfflineModeUpdater;
		}
	}

	public void CreateKarmaUpdate(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario, List<IBattleLogRuntime> battleLogList, int randomSeedByMonth, int[] projectileGroupIndicesFromServer, IDataProvider dataProvider)
	{
		if (_battleInstance != null && _campaignUpdater == null && dataProvider != null && dataProvider.GetBattleType() == BattleType.KarmaRaid)
		{
			_campaignUpdater = new KarmaUpdater(_battleInstance, chapterIndex, dungeonIndex, difficulty, isScenario, battleLogList, randomSeedByMonth, projectileGroupIndicesFromServer);
			_campaignUpdater.SetWaveReadyCallback(delegate
			{
				_campaignWaveReady = true;
			});
			_campaignUpdater.SetWaveEndCallback(delegate
			{
				_campaignWaveEnd = true;
			});
			_campaignUpdater.SetWavePauseCallback(delegate
			{
				_campaignWavePause = true;
				_campaignUpdater.SetWavePause(isPause: true);
			});
			_karmaUpdater = _campaignUpdater as KarmaUpdater;
		}
	}

	public void CreateCampaignUpdate(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario, List<IBattleLogRuntime> battleLogList, int randomSeedByMonth, int[] projectileGroupIndicesFromServer)
	{
		if (_battleInstance != null && _campaignUpdater == null)
		{
			_campaignUpdater = new CampaignUpdater(_battleInstance, chapterIndex, dungeonIndex, difficulty, isScenario, battleLogList, randomSeedByMonth, projectileGroupIndicesFromServer);
			_campaignUpdater.SetWaveReadyCallback(delegate
			{
				_campaignWaveReady = true;
			});
			_campaignUpdater.SetWaveEndCallback(delegate
			{
				_campaignWaveEnd = true;
			});
		}
	}

	public void StartThread()
	{
		if (_systemThread != null)
		{
			_systemThread.StartThread();
		}
	}

	public void Close()
	{
		StopBattleThread();
		_battleInstance = null;
		_eventRaiser = null;
		_eventRouter = null;
	}

	public void StopBattleThread()
	{
		if (_systemThread != null)
		{
			_systemThread.StopThread();
			_systemThread = null;
		}
	}

	public BattleInstance GetStopedBattleInstance()
	{
		StopBattleThread();
		return _battleInstance;
	}

	public ThreadContext DequeueThreadContext()
	{
		lock (_threadContextQueueSync)
		{
			if (_threadContextQueue.Count == 0)
			{
				return null;
			}
			return _threadContextQueue.Dequeue();
		}
	}

	public void AdvanceOfflineTime(int elapsedTick)
	{
		Interlocked.Add(ref _offlineElapsedTick, elapsedTick);
	}

	public int ThreadUpdateOnline()
	{
		if (_battleInstance == null)
		{
			return 0;
		}
		DateTime now = DateTime.Now;
		_battleInstance.Update();
		ThreadExecuteFlushJob();
		return (int)(DateTime.Now - now).TotalMilliseconds;
	}

	public int ThreadUpdateOffline()
	{
		if (_offlineElapsedTick < _battleInstance.DeltaTimeMs)
		{
			return 0;
		}
		DateTime now = DateTime.Now;
		int num = 0;
		while (_offlineElapsedTick >= _battleInstance.DeltaTimeMs && num < 10 && !((DateTime.Now - now).TotalSeconds > 0.10000000149011612))
		{
			_battleInstance.UpdateTick(_battleInstance.SnapshotEnabled);
			num++;
			Interlocked.Add(ref _offlineElapsedTick, -_battleInstance.DeltaTimeMs);
		}
		ThreadExecuteFlushJob();
		return (int)(DateTime.Now - now).TotalMilliseconds;
	}

	public int ThreadUpdateOnlineEnhance()
	{
		if (_battleInstance == null)
		{
			return 0;
		}
		DateTime now = DateTime.Now;
		_battleInstance.EnhancedUpdate();
		ThreadExecuteFlushJob();
		return (int)(DateTime.Now - now).TotalMilliseconds;
	}

	private void ThreadExecuteFlushJob()
	{
		_eventRaiser.FlushQueue();
		using (new SnapshotWritableBlock())
		{
			SnapshotObjectPoolContainer.FlushPendingRelease();
		}
		ThreadContext workerThreadContext = WorkerThreadContext;
		workerThreadContext.AssignCache(_battleInstance);
		if (_campaignUpdater != null)
		{
			_campaignUpdater.Update();
			workerThreadContext.AssignCampaignCache(_campaignUpdater, _campaignWaveReady, _campaignWaveEnd, _campaignWavePause);
			if (_eclipseUpdater != null)
			{
				workerThreadContext.AssignEclipseCache(_eclipseUpdater);
			}
			if (_karmaUpdater != null)
			{
				workerThreadContext.AssignKarmaCache(_karmaUpdater);
			}
			_campaignWaveReady = false;
			_campaignWaveEnd = false;
		}
		lock (_threadContextQueueSync)
		{
			_threadContextQueue.Enqueue(_workerThreadContext);
			_workerThreadContext = null;
		}
		_threadActionQueue.Receiver.Sync();
		while (!_threadActionQueue.Receiver.IsEmpty())
		{
			IThreadAction threadAction = _threadActionQueue.Receiver.Dequeue();
			threadAction.Action(_battleInstance, _campaignUpdater);
			threadAction.IsWaiting = false;
		}
	}

	private bool IsEnhanceModeContent()
	{
		if (_battleInstance == null)
		{
			return false;
		}
		string[] contentsStringArray = SingletonAuto<ContentsValueManager>.instance.GetContentsStringArray("EnhanceModeContentList");
		if (__Extension_Method_ICollection__.IsNullOrEmpty(contentsStringArray))
		{
			return false;
		}
		bool flag = false;
		string value = _battleInstance.BattleDefine.Code.GetValue();
		if (contentsStringArray.Contains(value))
		{
			flag = true;
		}
		BattleThreadDebugger instance = GlobalAutoBehaviour<BattleThreadDebugger>.instance;
		if (instance == null)
		{
			return flag;
		}
		instance.IsEnhancedContent = flag;
		if (!flag)
		{
			return instance.EnableForceEnhanceMode;
		}
		return true;
	}

	private void GraphLog(int elpaseTime)
	{
	}
}
