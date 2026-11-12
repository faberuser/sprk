using System;
using System.Collections.Generic;

namespace NShared;

public class BattleLogFactory
{
	public delegate string BattleLogSerializer(IBattleLogRuntime runtime);

	public delegate object BattleLogDeserializer(Type runtimeType, string jsonString);

	private readonly Dictionary<BattleLogType, Func<IBattleLogRuntime>> _runtimeDictionary;

	private BattleLogSerializer _serializer;

	private BattleLogDeserializer _deserializer;

	public BattleLogFactory()
	{
		_runtimeDictionary = new Dictionary<BattleLogType, Func<IBattleLogRuntime>>();
		Register(new PlayerEnterBattleLog());
		Register(new PlayerLeaveBattleLog());
		Register(new CreatureFireSkillBattleLog());
		Register(new CreatureFireEventSkillBattleLog());
		Register(new CreatureFireNextSkillBattleLog());
		Register(new CreatureReserveOnSkillBattleLog());
		Register(new CreatureReserveOffSkillBattleLog());
		Register(new CreatureSwitchBattleLog());
		Register(new CreatureCreateBattleLog());
		Register(new CreatureManualTargetBattleLog());
		Register(new CreatureManualMoveBattleLog());
		Register(new CreatureWakeUpBattleLog());
		Register(new CreatureSetAiBattleLog());
		Register(new WaveCreateBattleLog());
		Register(new WaveReadyBattleLog());
		Register(new WaveStartBattleLog());
		Register(new WaveRageModeBattleLog());
		Register(new WaveCloseBattleLog());
		Register(new WaveEndBattleLog());
		Register(new WaveWithdrawBattleLog());
		Register(new WaveGiveUpBattleLog());
		Register(new WaveCancelBattleLog());
		Register(new NextTeamEnterBattleLog());
		Register(new NextDungeonStartBattleLog());
		Register(new InitializeDungeonBattleLog());
		Register(new CreatureReserveOnDungeonSkillBattleLog());
		Register(new KarmaInitBattleLog());
		Register(new KarmaSelectCardBattleLog());
	}

	public void SetSerializer(BattleLogSerializer serializer)
	{
		_serializer = serializer;
	}

	public void SetDeserializer(BattleLogDeserializer deserializer)
	{
		_deserializer = deserializer;
	}

	private void Register<T>(T runtime) where T : IBattleLogRuntime, new()
	{
		_runtimeDictionary.Remove(runtime.GetBattleLogType());
		_runtimeDictionary.Add(runtime.GetBattleLogType(), () => new T());
	}

	public string SerializeBattleLog(IBattleLogRuntime runtime)
	{
		if (runtime == null)
		{
			return null;
		}
		return _serializer(runtime);
	}

	public bool IsRegistered(BattleLogType type)
	{
		Func<IBattleLogRuntime> value = null;
		_runtimeDictionary.TryGetValue(type, out value);
		return value != null;
	}

	public void UnitTest()
	{
		foreach (BattleLogType value in Enum.GetValues(typeof(BattleLogType)))
		{
			if (value != BattleLogType.None)
			{
				IsRegistered(value);
			}
		}
	}

	public IBattleLogRuntime Create(BattleLogType type, string data)
	{
		Func<IBattleLogRuntime> value = null;
		_runtimeDictionary.TryGetValue(type, out value);
		if (value == null)
		{
			return null;
		}
		IBattleLogRuntime battleLogRuntime = value();
		if (battleLogRuntime == null)
		{
			return null;
		}
		if (!(_deserializer(battleLogRuntime.GetType(), data) is IBattleLogRuntime result))
		{
			return null;
		}
		return result;
	}
}
