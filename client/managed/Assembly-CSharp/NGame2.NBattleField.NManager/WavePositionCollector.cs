using System.Collections.Generic;
using NGame2.NBattleView;
using NGame2.NBattleView.NCreature.NHandler;
using NGame2.NMessage;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleField.NManager;

public class WavePositionCollector : SceneAutoBehaviour<WavePositionCollector>
{
	private IncreaseDic<int, NShared.SetPosition> _dicWaveReady = new IncreaseDic<int, NShared.SetPosition>();

	private IncreaseDic<int, NShared.SetPosition> _dicDungeonChangeMove = new IncreaseDic<int, NShared.SetPosition>();

	private IncreaseDic<int, NShared.SetPosition> _dicDungeonChangeMoveBackward = new IncreaseDic<int, NShared.SetPosition>();

	public void BeginCollect()
	{
		BattleViewGenericSingleton<CreatureManager>.instance.EnableSyncPosition(enabled: false);
	}

	public void EndCollect()
	{
		BattleViewGenericSingleton<CreatureManager>.instance.EnableSyncPosition(enabled: true);
	}

	public void Clear(NShared.SetPosition.SetPositionMethodType method = NShared.SetPosition.SetPositionMethodType.None)
	{
		switch (method)
		{
		case NShared.SetPosition.SetPositionMethodType.WaveReady:
			_dicWaveReady.Clear();
			break;
		case NShared.SetPosition.SetPositionMethodType.DungeonChangeMove:
			_dicDungeonChangeMove.Clear();
			break;
		case NShared.SetPosition.SetPositionMethodType.DungeonChangeMoveBackward:
			_dicDungeonChangeMoveBackward.Clear();
			break;
		case NShared.SetPosition.SetPositionMethodType.None:
			_dicDungeonChangeMoveBackward.Clear();
			_dicDungeonChangeMove.Clear();
			_dicWaveReady.Clear();
			break;
		}
	}

	public List<Looting> CreateLootingOnly()
	{
		List<Looting> list = new List<Looting>();
		int creatureCount = BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureCount();
		for (int i = 0; i < creatureCount; i++)
		{
			Creature creatureByIndex = BattleViewGenericSingleton<CreatureManager>.instance.GetCreatureByIndex(i);
			if (!(creatureByIndex == null) && !creatureByIndex.IsDead && creatureByIndex.IsLeftTeam)
			{
				Looting item = new Looting(creatureByIndex);
				list.Add(item);
			}
		}
		AddLootingToHandler(list);
		for (int j = 0; j < list.Count; j++)
		{
			Looting looting = list[j];
			looting.Owner.NodeTreeController.SetExternalHandler(looting);
		}
		return list;
	}

	public List<NGame2.NBattleView.NCreature.NHandler.WaveReady> CreateHandler_WaveReady(bool includeRightTeam = false)
	{
		List<NGame2.NBattleView.NCreature.NHandler.WaveReady> list = new List<NGame2.NBattleView.NCreature.NHandler.WaveReady>();
		for (int i = 0; i < _dicWaveReady.Count; i++)
		{
			KeyValuePair<int, NShared.SetPosition> keyValuePairByIndex = _dicWaveReady.GetKeyValuePairByIndex(i);
			int key = keyValuePairByIndex.Key;
			NShared.SetPosition value = keyValuePairByIndex.Value;
			Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(key);
			if (!(creature == null) && !creature.IsDead && creature.IsLeftTeam)
			{
				NGame2.NBattleView.NCreature.NHandler.WaveReady item = new NGame2.NBattleView.NCreature.NHandler.WaveReady(creature, value);
				list.Add(item);
			}
		}
		AddLootingToHandler(list);
		float num = 0.1f;
		for (int j = 0; j < list.Count; j++)
		{
			NGame2.NBattleView.NCreature.NHandler.WaveReady waveReady = list[j];
			waveReady.CalculateTimeLength();
			num = Mathf.Max(num, waveReady.TimeLength);
		}
		for (int k = 0; k < list.Count; k++)
		{
			NGame2.NBattleView.NCreature.NHandler.WaveReady waveReady2 = list[k];
			waveReady2.SetMaxTimeLength(num);
			waveReady2.Owner.NodeTreeController.SetExternalHandler(waveReady2);
		}
		List<NGame2.NBattleView.NCreature.NHandler.WaveReady> list2 = new List<NGame2.NBattleView.NCreature.NHandler.WaveReady>();
		for (int l = 0; l < _dicWaveReady.Count; l++)
		{
			KeyValuePair<int, NShared.SetPosition> keyValuePairByIndex2 = _dicWaveReady.GetKeyValuePairByIndex(l);
			int key2 = keyValuePairByIndex2.Key;
			NShared.SetPosition value2 = keyValuePairByIndex2.Value;
			Creature creature2 = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(key2);
			if (!(creature2 == null) && !creature2.IsDead && !creature2.IsLeftTeam)
			{
				NGame2.NBattleView.NCreature.NHandler.WaveReady item2 = new NGame2.NBattleView.NCreature.NHandler.WaveReady(creature2, value2);
				list2.Add(item2);
			}
		}
		if (includeRightTeam)
		{
			num = 0.1f;
			for (int m = 0; m < list2.Count; m++)
			{
				NGame2.NBattleView.NCreature.NHandler.WaveReady waveReady3 = list2[m];
				waveReady3.CalculateTimeLength();
				num = Mathf.Max(num, waveReady3.TimeLength);
			}
			for (int n = 0; n < list2.Count; n++)
			{
				NGame2.NBattleView.NCreature.NHandler.WaveReady waveReady4 = list2[n];
				waveReady4.SetMaxTimeLength(num);
				waveReady4.Owner.NodeTreeController.SetExternalHandler(waveReady4);
			}
			list.AddRange(list2);
		}
		return list;
	}

	private void AddLootingToHandler<T>(List<T> handlerList) where T : ILooting
	{
		if (handlerList.Count == 0)
		{
			return;
		}
		List<DropItem> dropList = BattleViewGenericSingleton<NGame2.NBattleView.DropItemManager>.instance.DropList;
		while (dropList.Count > 0)
		{
			int index = 0;
			int index2 = 0;
			float num = float.MaxValue;
			for (int i = 0; i < dropList.Count; i++)
			{
				DropItem dropItem = dropList[i];
				for (int j = 0; j < handlerList.Count; j++)
				{
					float num2 = handlerList[j].CalculateIfAdd(dropItem);
					if (num2 < num)
					{
						num = num2;
						index = i;
						index2 = j;
					}
				}
			}
			DropItem dropItem2 = dropList[index];
			dropList.RemoveAt(index);
			handlerList[index2].AddDropItem(dropItem2);
		}
	}

	public List<DungeonChangeMove> CreateHandler_DungeonChangeMove()
	{
		List<DungeonChangeMove> list = new List<DungeonChangeMove>();
		for (int i = 0; i < _dicDungeonChangeMove.Count; i++)
		{
			KeyValuePair<int, NShared.SetPosition> keyValuePairByIndex = _dicDungeonChangeMove.GetKeyValuePairByIndex(i);
			int key = keyValuePairByIndex.Key;
			NShared.SetPosition value = keyValuePairByIndex.Value;
			Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(key);
			if (!(creature == null) && !creature.IsDead && creature.IsLeftTeam)
			{
				DungeonChangeMove dungeonChangeMove = new DungeonChangeMove(creature, value);
				dungeonChangeMove.Owner.NodeTreeController.SetExternalHandler(dungeonChangeMove);
				list.Add(dungeonChangeMove);
			}
		}
		return list;
	}

	public List<DungeonChangeMoveBackward> CreateHandler_DungeonChangeMoveBackward()
	{
		List<DungeonChangeMoveBackward> list = new List<DungeonChangeMoveBackward>();
		for (int i = 0; i < _dicDungeonChangeMoveBackward.Count; i++)
		{
			KeyValuePair<int, NShared.SetPosition> keyValuePairByIndex = _dicDungeonChangeMoveBackward.GetKeyValuePairByIndex(i);
			int key = keyValuePairByIndex.Key;
			NShared.SetPosition value = keyValuePairByIndex.Value;
			Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(key);
			if (!(creature == null) && !creature.IsDead && creature.IsLeftTeam)
			{
				DungeonChangeMoveBackward dungeonChangeMoveBackward = new DungeonChangeMoveBackward(creature, value);
				dungeonChangeMoveBackward.Owner.NodeTreeController.SetExternalHandler(dungeonChangeMoveBackward);
				list.Add(dungeonChangeMoveBackward);
			}
		}
		return list;
	}

	public void OnMessageFast(CallbackMessage<NShared.SetPosition> message)
	{
		if (message.IsValidMessage())
		{
			if (message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.WaveReady)
			{
				_dicWaveReady.AddAndOverride(message.Callback.CreatureUid, message.Callback);
			}
			else if (message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.DungeonChangeMove)
			{
				_dicDungeonChangeMove.AddAndOverride(message.Callback.CreatureUid, message.Callback);
			}
			else if (message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.DungeonChangeMoveBackward)
			{
				_dicDungeonChangeMoveBackward.AddAndOverride(message.Callback.CreatureUid, message.Callback);
			}
		}
	}

	public bool IsContain(NShared.SetPosition.SetPositionMethodType method)
	{
		return method switch
		{
			NShared.SetPosition.SetPositionMethodType.WaveReady => __Extension_Method_ICollection__.COUNT(_dicWaveReady) > 0, 
			NShared.SetPosition.SetPositionMethodType.DungeonChangeMove => __Extension_Method_ICollection__.COUNT(_dicDungeonChangeMove) > 0, 
			NShared.SetPosition.SetPositionMethodType.DungeonChangeMoveBackward => __Extension_Method_ICollection__.COUNT(_dicDungeonChangeMoveBackward) > 0, 
			_ => false, 
		};
	}
}
