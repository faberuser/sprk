using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniJSON;
using NShared.NUtil;
using NVespa;

namespace NShared.NCampaign;

public class EclipseOfflineModeUpdater : CampaignUpdater
{
	public class DataForSendingToFacade<T>
	{
		private T _value;

		public bool IsChanged { get; internal set; }

		public T Value
		{
			get
			{
				IsChanged = false;
				return _value;
			}
			set
			{
				_value = value;
			}
		}
	}

	public interface IFuncEclipse
	{
		int InitEclipseDeckData(ref Dictionary<int, List<BattleCreatureCreateInfo>> EclipseDeckData);

		void InitEclipseDungeonData(ref EclipseDungeonData[] EclipseDungeonData);

		void InitPlayerEnterBattleLog(ref PlayerEnterBattleLog PlayerEnterLog, int[] heroIndices);

		int GetEclipseGameSpeedRatio();

		int GetEclipseInitWaveIndex();

		int GetEclipseMaxWaveIndex();

		bool GetIsContinousBattle();

		long GetAccountID();

		Func<object, string> GetJsonMarshalerEncodeJsonString();

		BaseJsonMarshaler.EncoderDelegate<ResultCreatureInfo> GetJsonMarshalerEncodeJsonObject();
	}

	private Dictionary<int, List<BattleCreatureCreateInfo>> CreatureCreateBattleLogList;

	private EclipseDungeonData[] DungeonList;

	private int _currentDeckIndex = 1;

	private int _maxDeckIndex;

	private int _currentLevel;

	private long _accountID;

	private int _lastDungeonBattleLogNo = -1;

	private int _clearWaveBattleLogNo = -1;

	private bool _isFirstWaveClear;

	private List<ResultCreatureInfo> _playerClearedInfoList;

	private int _clearCreatureUID = -1;

	private List<BaseBattleLog> _tempBaseBattleLogList;

	private List<ResultCreatureInfo> _tempCreatureInfoList;

	private HashSet<int> _tempHashSetInt;

	private StringBuilder _keyBuilder;

	private Queue<ResultCreatureInfo> _resultCreatureInfoQueue;

	private Func<object, string> JsonMarshalerEncodeJsonString;

	private BaseJsonMarshaler.EncoderDelegate<ResultCreatureInfo> JsonMarshalerEncodeJsonObject;

	private static int EclipseDungeonIncreaseLevelStep = -1;

	private const string EclipseDungeonIncreaseLevelStepKey = "EclipseDungeonIncreaseLevelStep";

	private DataForSendingToFacade<bool> IsContinuousBattle = new DataForSendingToFacade<bool>();

	private DataForSendingToFacade<EclipseStatus> SendEclipseStatus = new DataForSendingToFacade<EclipseStatus>();

	public int InitialDungeonIndex { get; private set; }

	public int InitialChapterIndex { get; private set; }

	public int InitialWaveIndex { get; private set; }

	public int InitialMaxWaveIndex { get; private set; }

	public int ResultData_PlayTime { get; private set; }

	public long ResultData_TotalDamage { get; private set; }

	public int ResultData_DeckIndex { get; private set; }

	public int ResultData_CreatureLevel { get; private set; }

	public int ResultData_ChapterIndex { get; private set; }

	public int ResultData_DungeonIndex { get; private set; }

	public int ResultData_WaveIndex { get; private set; }

	public int ResultData_MaxWaveIndex { get; private set; }

	public string BattleLogString { get; private set; }

	public string CreatureInfoString { get; private set; }

	public EclipseOfflineModeUpdater(BattleInstance battleInstance, int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario, List<IBattleLogRuntime> initialBattleLogList, int randomSeedByMonth, IFuncEclipse FuncEclipse, int[] projectileGroupIndicesFromServer = null)
		: base(battleInstance, chapterIndex, dungeonIndex, difficulty, scenario, initialBattleLogList, randomSeedByMonth, projectileGroupIndicesFromServer)
	{
		_currentDeckIndex = FuncEclipse.InitEclipseDeckData(ref CreatureCreateBattleLogList);
		FuncEclipse.InitEclipseDungeonData(ref DungeonList);
		SetInitialChapterIndex(chapterIndex);
		SetInitialDungeonIndex(dungeonIndex);
		SetInitialWaveIndex(FuncEclipse.GetEclipseInitWaveIndex());
		SetInitialMaxWaveIndex(FuncEclipse.GetEclipseMaxWaveIndex());
		Set_ContinuousBattle(FuncEclipse.GetIsContinousBattle(), SetChanged: false);
		SetAccountID(FuncEclipse.GetAccountID());
		SetJsonMarshalerEncodeJsonString(FuncEclipse);
		SetJsonMarshalerEncodeJsonObject(FuncEclipse);
		Init();
	}

	private void Init()
	{
		SetWavePauseCallback(WavePause);
		SetBattleEndCallback(BattleEnd);
		SetWaveIndex(InitialWaveIndex);
		SetMaxWaveIndex(InitialMaxWaveIndex);
		_maxDeckIndex = CreatureCreateBattleLogList.Keys.Max();
		_keyBuilder = new StringBuilder();
		_tempHashSetInt = new HashSet<int>();
		_tempCreatureInfoList = new List<ResultCreatureInfo>();
		_playerClearedInfoList = new List<ResultCreatureInfo>();
		_resultCreatureInfoQueue = new Queue<ResultCreatureInfo>(4);
		_isFirstWaveClear = false;
	}

	private void Release()
	{
		_tempBaseBattleLogList.Clear();
		_tempCreatureInfoList.Clear();
		_tempHashSetInt.Clear();
		_resultCreatureInfoQueue.Clear();
		_playerClearedInfoList.Clear();
		JsonMarshalerEncodeJsonString = null;
		JsonMarshalerEncodeJsonObject = null;
		DungeonList = null;
		foreach (KeyValuePair<int, List<BattleCreatureCreateInfo>> creatureCreateBattleLog in CreatureCreateBattleLogList)
		{
			creatureCreateBattleLog.Value.Clear();
		}
	}

	public static EclipseStatus GetHighPriorityStatus(EclipseStatus eclipseStatus_old, EclipseStatus eclipseStatus_new)
	{
		if (eclipseStatus_old == EclipseStatus.BattleOut || eclipseStatus_new == EclipseStatus.BattleOut)
		{
			return EclipseStatus.BattleOut;
		}
		if (eclipseStatus_old == EclipseStatus.BattleEnd || eclipseStatus_new == EclipseStatus.BattleEnd)
		{
			return EclipseStatus.BattleEnd;
		}
		return eclipseStatus_old;
	}

	public void Init_EclipseStatus()
	{
		SendEclipseStatus.Value = EclipseStatus.None;
		SendEclipseStatus.IsChanged = false;
	}

	public bool Get_ContinuousBattle()
	{
		return IsContinuousBattle.Value;
	}

	public bool Get_ContinuousBattleChanged()
	{
		return IsContinuousBattle.IsChanged;
	}

	public void Set_ContinuousBattle(bool Value, bool SetChanged = true)
	{
		IsContinuousBattle.Value = Value;
		IsContinuousBattle.IsChanged = SetChanged;
	}

	public EclipseStatus Get_EclipseStatus()
	{
		return SendEclipseStatus.Value;
	}

	public bool Get_EclipseStatusChanged()
	{
		return SendEclipseStatus.IsChanged;
	}

	public void Set_EclipseStatus(EclipseStatus Value)
	{
		if (SendEclipseStatus.IsChanged)
		{
			EclipseStatus value = SendEclipseStatus.Value;
			Value = GetHighPriorityStatus(value, Value);
			if (value == Value)
			{
				SendEclipseStatus.IsChanged = true;
				return;
			}
		}
		SendEclipseStatus.Value = Value;
		SendEclipseStatus.IsChanged = true;
		ResultData_PlayTime = (int)(base.BattleInstance.GlobalTimer.GetCurrentTimeMs() / 1000);
		ResultData_TotalDamage = base.BattleInstance.Snapshot.StatisticsManager.GetTotalDamageOfAccount(_accountID);
		ResultData_ChapterIndex = base.ChapterIndex;
		ResultData_DungeonIndex = base.DungeonIndex;
		ResultData_WaveIndex = base.WaveIndex;
		ResultData_MaxWaveIndex = base.MaxWaveIndex;
		ResultData_DeckIndex = _currentDeckIndex;
		ResultData_CreatureLevel = _currentLevel;
		if (_isFirstWaveClear)
		{
			SetLogs(Value);
		}
	}

	private void SetLogs(EclipseStatus Value)
	{
		switch (Value)
		{
		case EclipseStatus.BattleContinue:
			BattleLogString = GetBattleLogString();
			CreatureInfoString = GetCreatureInfoString(isContinue: true);
			SetFirstClearWave(isClear: false);
			break;
		case EclipseStatus.BattleEnd:
		case EclipseStatus.BattleTeamEndOff:
		case EclipseStatus.BattleTeamEndOn:
		case EclipseStatus.BattleOut:
			BattleLogString = GetBattleLogString(_clearWaveBattleLogNo);
			CreatureInfoString = GetCreatureInfoString(isContinue: false);
			SetFirstClearWave(isClear: false);
			Release();
			break;
		}
	}

	public string GetBattleLogString(int noEnd = -1)
	{
		return CreateEclipseBattleLogString(_lastDungeonBattleLogNo, out _lastDungeonBattleLogNo, noEnd);
	}

	public string CreateEclipseBattleLogString(int noStart, out int saveEndNo, int noEnd = -1)
	{
		base.BattleInstance.BattleLogContainer.GetEclipseBattleLogList(ref _tempBaseBattleLogList, noStart, noEnd);
		saveEndNo = base.BattleInstance.BattleLogContainer.GetNewestLogNo();
		_keyBuilder.Clear();
		_keyBuilder.Append('[');
		for (int i = 0; i < _tempBaseBattleLogList.Count; i++)
		{
			BaseBattleLog arg = _tempBaseBattleLogList[i];
			string value = JsonMarshalerEncodeJsonString(arg);
			_keyBuilder.Append(value);
			if (i != _tempBaseBattleLogList.Count - 1)
			{
				_keyBuilder.Append(',');
			}
		}
		_keyBuilder.Append(']');
		return Lz4.Compress(_keyBuilder.ToString());
	}

	public string GetCreatureInfoString(bool isContinue)
	{
		InitCreatureInfoList(isContinue);
		return Json.Serialize(BaseJsonMarshaler.EncodeArray(_tempCreatureInfoList, JsonMarshalerEncodeJsonObject));
	}

	public void InitCreatureInfoList(bool isContinue)
	{
		_tempCreatureInfoList.Clear();
		BattleCreatureList creatures = base.BattleInstance.CreatureContainer.GetCreatures();
		if (!isContinue)
		{
			_tempCreatureInfoList.AddRange(_playerClearedInfoList);
		}
		_playerClearedInfoList.Clear();
		for (int i = 0; i < creatures.Count; i++)
		{
			BattleCreature battleCreature = creatures[i];
			int uid = battleCreature.Uid;
			_tempHashSetInt.Add(uid);
			if (isContinue || (uid <= _clearCreatureUID && battleCreature.TeamController.TeamId != 0))
			{
				ResultCreatureInfo resultCreatureInfo = CreateCreatureInfo(battleCreature);
				if (resultCreatureInfo != null)
				{
					_tempCreatureInfoList.Add(resultCreatureInfo);
				}
			}
		}
		Dictionary<int, ResultCreatureInfo> destroyedCreatureInfos = base.BattleInstance.CreatureContainer.GetDestroyedCreatureInfos();
		if (destroyedCreatureInfos != null)
		{
			foreach (KeyValuePair<int, ResultCreatureInfo> item in destroyedCreatureInfos)
			{
				int key = item.Key;
				ResultCreatureInfo value = item.Value;
				if (_tempHashSetInt.Contains(key))
				{
					VespaDebug.LogError("Alive creature exists in destroyed-list!");
				}
				else if (isContinue || (key <= _clearCreatureUID && value.TeamId != 0))
				{
					_tempCreatureInfoList.Add(value);
				}
			}
		}
		_tempHashSetInt.Clear();
	}

	public ResultCreatureInfo CreateCreatureInfo(BattleCreature creature)
	{
		if (creature == null)
		{
			return null;
		}
		if (creature.SummonerUid != 0)
		{
			return null;
		}
		int teamId = creature.TeamController.TeamId;
		ResultCreatureInfo resultCreatureInfo;
		if (_resultCreatureInfoQueue.Count > 0)
		{
			resultCreatureInfo = _resultCreatureInfoQueue.Dequeue();
			resultCreatureInfo.Index = creature.CreatureIndex;
			resultCreatureInfo.TeamId = teamId;
			resultCreatureInfo.Key = ResultCreatureInfo.GetKey(creature.CreatureIndex, creature.CreateInfo.WaveSlotIndex, teamId);
			resultCreatureInfo.Hp = creature.StatController.Hp;
			resultCreatureInfo.Mp = creature.StatController.Mp;
			resultCreatureInfo.MaxHp = creature.StatController.GetMaxHp();
			resultCreatureInfo.MaxMp = creature.StatController.GetMaxMp();
			resultCreatureInfo.GivedDamage = GetGivedDamage(creature);
		}
		else
		{
			resultCreatureInfo = new ResultCreatureInfo
			{
				Index = creature.CreatureIndex,
				TeamId = teamId,
				Key = ResultCreatureInfo.GetKey(creature.CreatureIndex, creature.CreateInfo.WaveSlotIndex, teamId),
				Hp = creature.StatController.Hp,
				Mp = creature.StatController.Mp,
				MaxHp = creature.StatController.GetMaxHp(),
				MaxMp = creature.StatController.GetMaxMp(),
				GivedDamage = GetGivedDamage(creature)
			};
		}
		return resultCreatureInfo;
	}

	private long GetGivedDamage(BattleCreature creature)
	{
		if (creature == null || creature.StatController.GetMinHp() == 0L)
		{
			return 0L;
		}
		return creature.BattleInstance.Snapshot.StatisticsManager.TotalDamagedMeter(creature.Uid);
	}

	public void RenewClearWave()
	{
		_clearWaveBattleLogNo = base.BattleInstance.BattleLogContainer.GetNewestLogNo();
		for (int i = 0; i < _playerClearedInfoList.Count; i++)
		{
			_resultCreatureInfoQueue.Enqueue(_playerClearedInfoList[i]);
		}
		_playerClearedInfoList.Clear();
		_clearCreatureUID = 0;
		BattleCreatureList creatures = base.BattleInstance.CreatureContainer.GetCreatures();
		for (int j = 0; j < creatures.Count; j++)
		{
			BattleCreature battleCreature = creatures[j];
			if (battleCreature.TeamController.TeamType == TeamType.Left)
			{
				ResultCreatureInfo resultCreatureInfo = CreateCreatureInfo(battleCreature);
				if (resultCreatureInfo != null)
				{
					_playerClearedInfoList.Add(resultCreatureInfo);
				}
			}
			else
			{
				_clearCreatureUID = System.Math.Max(battleCreature.Uid, _clearCreatureUID);
			}
		}
		Dictionary<int, ResultCreatureInfo> destroyedCreatureInfos = base.BattleInstance.CreatureContainer.GetDestroyedCreatureInfos();
		if (destroyedCreatureInfos == null)
		{
			return;
		}
		foreach (KeyValuePair<int, ResultCreatureInfo> item in destroyedCreatureInfos)
		{
			ResultCreatureInfo value = item.Value;
			if (value.TeamId == 0)
			{
				_playerClearedInfoList.Add(value);
			}
			else
			{
				_clearCreatureUID = System.Math.Max(item.Key, _clearCreatureUID);
			}
		}
	}

	private void SetAccountID(long accountID)
	{
		_accountID = accountID;
	}

	private void SetJsonMarshalerEncodeJsonString(IFuncEclipse eclipseContext)
	{
		JsonMarshalerEncodeJsonString = eclipseContext.GetJsonMarshalerEncodeJsonString();
	}

	private void SetJsonMarshalerEncodeJsonObject(IFuncEclipse eclipseContext)
	{
		JsonMarshalerEncodeJsonObject = eclipseContext.GetJsonMarshalerEncodeJsonObject();
	}

	public int GetCurrentDeckIndex()
	{
		return _currentDeckIndex;
	}

	public void IncreaseCurrentDeckIndex()
	{
		_currentDeckIndex++;
	}

	public void SetMaxWaveIndex(int index)
	{
		base.MaxWaveIndex = index;
	}

	public int GetMaxWaveIndex()
	{
		return base.MaxWaveIndex;
	}

	public bool GetIsFinalDeck()
	{
		return _currentDeckIndex == _maxDeckIndex;
	}

	public void SwitchContinousBattle()
	{
		Set_ContinuousBattle(!Get_ContinuousBattle());
	}

	public void ForceNextTeam()
	{
		WaveWithdrawBattleLog runtime = new WaveWithdrawBattleLog();
		base.BattleInstance.BattleLogContainer.AddLog(runtime);
	}

	public void BattleOut()
	{
		Set_EclipseStatus(EclipseStatus.BattleOut);
	}

	public void SetInitialChapterIndex(int chapterIndex)
	{
		InitialChapterIndex = chapterIndex;
	}

	public void SetInitialDungeonIndex(int dungeonIndex)
	{
		InitialDungeonIndex = dungeonIndex;
	}

	public void SetInitialWaveIndex(int waveIndex)
	{
		InitialWaveIndex = waveIndex;
	}

	public void SetInitialMaxWaveIndex(int maxWaveIndex)
	{
		InitialMaxWaveIndex = maxWaveIndex;
	}

	public void SetFirstClearWave(bool isClear)
	{
		_isFirstWaveClear = isClear;
	}

	public void InitializeDungeonSetting()
	{
		base.ChapterIndex = InitialChapterIndex;
		base.DungeonIndex = InitialDungeonIndex;
		base.WaveIndex = InitialWaveIndex;
		base.MaxWaveIndex = InitialMaxWaveIndex;
		_currentLevel = 0;
	}

	public void SetWaveIndex(int index)
	{
		base.WaveIndex = index;
	}

	public bool TrySettingNextDungeon()
	{
		if (DungeonList == null)
		{
			return false;
		}
		int num = Array.FindIndex(DungeonList, (EclipseDungeonData data) => data.ChapterIndex == base.ChapterIndex && data.DungeonIndex == base.DungeonIndex);
		if (num <= -1 || num + 1 >= DungeonList.Length)
		{
			return false;
		}
		EclipseDungeonData eclipseDungeonData = DungeonList[num + 1];
		base.ChapterIndex = eclipseDungeonData.ChapterIndex;
		base.DungeonIndex = eclipseDungeonData.DungeonIndex;
		ResetWaveIndex();
		Log("Next Eclipse : chapter=" + base.ChapterIndex + " dungeon=" + base.DungeonIndex + " waveIndex=" + base.WaveIndex);
		return true;
	}

	public void ResetWaveIndex()
	{
		base.WaveIndex = 1;
	}

	public int IncreaseCreatureLevel()
	{
		_currentLevel++;
		return _currentLevel;
	}

	public int GetCurrentCreatureLevel()
	{
		return _currentLevel;
	}

	public List<BattleCreatureCreateInfo> PeekNextHeroDeck()
	{
		_currentDeckIndex++;
		List<BattleCreatureCreateInfo> currentHeroDeck = GetCurrentHeroDeck();
		_currentDeckIndex--;
		return currentHeroDeck;
	}

	public List<BattleCreatureCreateInfo> GetCurrentHeroDeck()
	{
		if (!CreatureCreateBattleLogList.ContainsKey(_currentDeckIndex))
		{
			return null;
		}
		return CreatureCreateBattleLogList[_currentDeckIndex];
	}

	public void Log(object message)
	{
	}

	public void WavePause(TeamType winner)
	{
		if (winner == TeamType.Left)
		{
			Set_EclipseStatus(EclipseStatus.BattleContinue);
		}
		SetWavePause(isPause: true);
	}

	private void BattleEnd()
	{
		if (GetIsFinalDeck())
		{
			Set_EclipseStatus(EclipseStatus.BattleEnd);
			return;
		}
		bool continuousBattleChanged = Get_ContinuousBattleChanged();
		if (Get_ContinuousBattle())
		{
			Set_EclipseStatus(EclipseStatus.BattleTeamEndOn);
		}
		else
		{
			Set_EclipseStatus(EclipseStatus.BattleTeamEndOff);
		}
		IsContinuousBattle.IsChanged = continuousBattleChanged;
	}

	public static int GetEclipseDungeonIncreaseLevelStep()
	{
		if (EclipseDungeonIncreaseLevelStep <= -1)
		{
			EclipseDungeonIncreaseLevelStep = GameRoot.Instance.ConstantDataContainer.GetValueInt("EclipseDungeonIncreaseLevelStep", 5);
		}
		return EclipseDungeonIncreaseLevelStep;
	}
}
