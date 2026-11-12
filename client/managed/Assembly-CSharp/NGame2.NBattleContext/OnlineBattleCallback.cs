using System;
using NGame2.NBattleField;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSocket;
using UnityEngine;

namespace NGame2.NBattleContext;

public class OnlineBattleCallback
{
	public Action<DestroyCreatureNot> OnCreatureDestroyedCallback;

	public Action<BattleResultNot> OnBattleResultCallback;

	public Action<BattleEndNot> OnBattleEndCallback;

	public Action<BattleBanPickStartNot> OnBattleBanPickStartCallback;

	public Action<BattleBanPickStartCancelNot> OnBattleBanPickStartCancelCallback;

	public Action<BattleBanPickNextNot> OnBattleBanPickNextCallback;

	public Action<BattleBanPickEndNot> OnBattleBanPickEndCallback;

	public Action<BattleBanPickTimeOutNot> OnBattleBanPickTimeOutCallback;

	public Action<BattleBanPickGiveUpNot> OnBattleBanPickGiveUpCallback;

	public Action<BattleMessageNot> OnBattleMessageCallback;

	public Action<SetNextDungeonNot> OnSetNextDungeonNotCallback;

	public Action<SetNextTeamNot> OnSetNextTeamNotCallback;

	public Action<WaveEndNot> OnWaveEndNotCallback;

	public Action<WaveReadyNot> OnWaveReadyNotCallback;

	public Action<BattleCancelNot> OnBattleCancelNotCallback;

	private IHandler _handler;

	public bool IsWaveEnd { get; set; }

	public DateTime WaveEndedTime { get; set; }

	public bool IsBattleEnd { get; private set; }

	public int WaveIndex { get; private set; }

	public int TotalWaveIndex { get; private set; }

	public bool IsSetNextDungeon { get; set; }

	public bool IsSetNextTeam { get; set; }

	public TeamType Winner { get; set; }

	public bool IsContinuousBattle { get; set; }

	public OnlineBattleCallback(IHandler handler)
	{
		_handler = handler;
	}

	public void Init()
	{
		_handler.Register<DestroyCreatureNot>(OnPacket);
		_handler.Register<BattleResultNot>(OnPacket);
		_handler.Register<BattleLogNot>(OnPacket);
		_handler.Register<WaveReadyNot>(OnPacket);
		_handler.Register<WaveEndNot>(OnPacket);
		_handler.Register<SetNextDungeonNot>(OnPacket);
		_handler.Register<SetNextTeamNot>(OnPacket);
		_handler.Register<BattleEndNot>(OnPacket);
		_handler.Register<BattleBanPickStartNot>(OnPacket);
		_handler.Register<BattleBanPickNextNot>(OnPacket);
		_handler.Register<BattleBanPickEndNot>(OnPacket);
		_handler.Register<BattleBanPickTimeOutNot>(OnPacket);
		_handler.Register<BattleBanPickGiveUpNot>(OnPacket);
		_handler.Register<BattleBanPickStartCancelNot>(OnPacket);
		_handler.Register<BattleMessageNot>(OnPacket);
		_handler.Register<ContinuousBattleNot>(OnPacket);
		_handler.Register<BattleCancelNot>(OnPacket);
	}

	public void Close()
	{
		_handler.Clear();
		IsWaveEnd = false;
		IsBattleEnd = false;
		WaveIndex = 1;
		TotalWaveIndex = 1;
		IsSetNextDungeon = false;
	}

	public void SetBattleEnd(bool _IsBattleEnd)
	{
		IsBattleEnd = _IsBattleEnd;
	}

	private void OnPacket(BattleLogNot data)
	{
		BaseBattleLog[] battleLogs = data.BattleLogs;
		if (battleLogs == null)
		{
			return;
		}
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			BaseBattleLog[] array = battleLogs;
			foreach (BaseBattleLog o in array)
			{
				Debug.LogError("BattleLog ignored : " + JsonMarshaler.EncodeJsonString(o));
			}
			return;
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(battleLogs); j++)
		{
		}
		if (ProfileManager.IsLogEnabled)
		{
			ProfileManager.RecordBattleLogDelta();
		}
		SingletonAuto<BattleFacade>.instance.AddReceivedLog(battleLogs);
	}

	private void OnPacket(WaveReadyNot data)
	{
		WaveIndex = data.WaveIndex;
		TotalWaveIndex = data.TotalWaveIndex;
		Vespa_System_Action_Extension_Method.fire(OnWaveReadyNotCallback, data);
	}

	private void OnPacket(WaveEndNot data)
	{
		IsWaveEnd = true;
		WaveEndedTime = DateTime.UtcNow;
		Winner = data.Winner;
		Vespa_System_Action_Extension_Method.fire(OnWaveEndNotCallback, data);
	}

	private void OnPacket(SetNextDungeonNot data)
	{
		Vespa_System_Action_Extension_Method.fire(OnSetNextDungeonNotCallback, data);
		WaveIndex = 0;
		TotalWaveIndex = 0;
		IsSetNextDungeon = true;
	}

	private void OnPacket(SetNextTeamNot data)
	{
		VespaDebug.LogError("SetNextTeamNot received.");
		Vespa_System_Action_Extension_Method.fire(OnSetNextTeamNotCallback, data);
		IsSetNextTeam = true;
	}

	private void OnPacket(BattleEndNot data)
	{
		Vespa_System_Action_Extension_Method.fire(OnBattleEndCallback, data);
	}

	private void OnPacket(DestroyCreatureNot data)
	{
		Vespa_System_Action_Extension_Method.fire(OnCreatureDestroyedCallback, data);
	}

	private void OnPacket(BattleResultNot data)
	{
		IsBattleEnd = true;
		Vespa_System_Action_Extension_Method.fire(OnBattleResultCallback, data);
	}

	private void OnPacket(BattleBanPickStartNot data)
	{
		Vespa_System_Action_Extension_Method.fire(OnBattleBanPickStartCallback, data);
	}

	private void OnPacket(BattleBanPickStartCancelNot data)
	{
		Vespa_System_Action_Extension_Method.fire(OnBattleBanPickStartCancelCallback, data);
	}

	private void OnPacket(BattleBanPickNextNot data)
	{
		Vespa_System_Action_Extension_Method.fire(OnBattleBanPickNextCallback, data);
	}

	private void OnPacket(BattleBanPickEndNot data)
	{
		Vespa_System_Action_Extension_Method.fire(OnBattleBanPickEndCallback, data);
	}

	private void OnPacket(BattleBanPickTimeOutNot data)
	{
		Vespa_System_Action_Extension_Method.fire(OnBattleBanPickTimeOutCallback, data);
	}

	private void OnPacket(BattleBanPickGiveUpNot data)
	{
		Vespa_System_Action_Extension_Method.fire(OnBattleBanPickGiveUpCallback, data);
	}

	private void OnPacket(BattleMessageNot data)
	{
		if (!SprkPartyRetry.OnMessage(data))
		{
			Vespa_System_Action_Extension_Method.fire(OnBattleMessageCallback, data);
		}
	}

	private void OnPacket(ContinuousBattleNot data)
	{
		IsContinuousBattle = data.IsContinuousBattle;
	}

	private void OnPacket(BattleCancelNot data)
	{
		IsWaveEnd = true;
		WaveEndedTime = DateTime.UtcNow;
		_ = data.Reason;
		Vespa_System_Action_Extension_Method.fire(OnBattleCancelNotCallback, data);
	}
}
