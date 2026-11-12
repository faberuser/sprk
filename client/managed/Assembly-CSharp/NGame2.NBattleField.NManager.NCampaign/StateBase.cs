using System;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleView;
using NGame2.NUI.NManager;
using NShared;
using NVespa;

namespace NGame2.NBattleField.NManager.NCampaign;

public abstract class StateBase
{
	public enum SwitchCameraType
	{
		WaveMove,
		WaveEnd
	}

	public interface IListener
	{
		SwitchCameraType[] GetIgnoreSwitchCameraTypes();

		void OnCampaignBegin();

		void OnCampaignReady();

		void OnCampaignEnd();

		void StartBattle();

		bool IsBattleEnd();

		int GetChapterIndex();

		int GetDungeonIndex();

		DungeonDifficultyType GetDifficulty();

		bool IsScenarioDungeon();

		int GetCurrentWaveIndex();

		bool IsWaveReady(bool reset);

		bool IsWaveEnd(int waveIndex, bool reset);

		bool IsWavePause(bool reset);

		string GetCurrentWaveBgm();

		BattleStartEffectData GetBattleStartEffectData();

		bool IsSetNextDungeon(bool reset);

		bool IsSetNextTeam(bool reset);

		TeamType WaveWinner();

		EnterDungeonData GetEnterDungeonData();

		void OnWaveResume(Action callback);

		bool IsContinuousBattle();
	}

	protected IListener _listener;

	protected StateBase _nextState;

	public IListener IListener_ => _listener;

	public void Next(StateBase nextState)
	{
		_nextState = nextState;
	}

	public StateBase PopNext()
	{
		if (_nextState == null)
		{
			return null;
		}
		StateBase nextState = _nextState;
		_nextState = null;
		return nextState;
	}

	public virtual void OnStart(IListener listener)
	{
		_listener = listener;
	}

	public virtual void OnEnd()
	{
	}

	public virtual void ChainUpdate()
	{
	}

	public static List<Creature> GetBossList(int checkWaveIndex)
	{
		if (!BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			return null;
		}
		List<Creature> list = new List<Creature>();
		for (int i = 0; i < BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureCount(); i++)
		{
			Creature creatureByIndex = BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureByIndex(i);
			if (!creatureByIndex.IsDead && !creatureByIndex.IsResting && creatureByIndex.CreateInfo.IsBoss && creatureByIndex.CreateWaveIndex <= checkWaveIndex)
			{
				list.Add(creatureByIndex);
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			return null;
		}
		return list;
	}

	public static Creature GetBossCreature(int checkWaveIndex)
	{
		if (!BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			return null;
		}
		for (int i = 0; i < BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureCount(); i++)
		{
			Creature creatureByIndex = BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureByIndex(i);
			if (!creatureByIndex.IsDead && !creatureByIndex.IsResting && creatureByIndex.CreateInfo.IsBoss && creatureByIndex.CreateWaveIndex <= checkWaveIndex)
			{
				return creatureByIndex;
			}
		}
		return null;
	}

	protected bool IsIgnoreSwitchCamera(SwitchCameraType camType)
	{
		SwitchCameraType[] ignoreSwitchCameraTypes = _listener.GetIgnoreSwitchCameraTypes();
		if (ignoreSwitchCameraTypes == null)
		{
			return false;
		}
		return Array.FindIndex(ignoreSwitchCameraTypes, (SwitchCameraType ignoreType) => ignoreType == camType) != -1;
	}

	protected void DebugLog(string log)
	{
		if (ClientValueManager.IsShowEclipseJobState)
		{
			VespaDebug.LogError(log);
		}
	}
}
