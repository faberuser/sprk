using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleField.NManager;

public class DropItemManager : SceneAutoBehaviour<DropItemManager>
{
	public struct CreatureKey
	{
		public class EqualComparer : IEqualityComparer<CreatureKey>
		{
			bool IEqualityComparer<CreatureKey>.Equals(CreatureKey x, CreatureKey y)
			{
				if (x.IsBoss == y.IsBoss && x.WaveIndex == y.WaveIndex)
				{
					return x.WaveSlotIndex == y.WaveSlotIndex;
				}
				return false;
			}

			int IEqualityComparer<CreatureKey>.GetHashCode(CreatureKey obj)
			{
				return (obj.IsBoss ? 10000 : 0) + obj.WaveIndex * 100 + obj.WaveSlotIndex;
			}
		}

		public bool IsBoss;

		public int WaveIndex;

		public int WaveSlotIndex;

		public CreatureKey(bool isBoss, int waveIndex, int waveSlotIndex)
		{
			this = default(CreatureKey);
			IsBoss = isBoss;
			WaveIndex = waveIndex;
			WaveSlotIndex = waveSlotIndex;
		}
	}

	public class CreatureDrop
	{
		public List<int> ItemList = new List<int>();

		public List<int> GoldList = new List<int>();

		public CampaignWaveData.CreatureDetailInfo CreatureInfo { get; private set; }

		public CreatureData CreatureData { get; private set; }

		public float DropItemWeight { get; private set; }

		public float DropGoldWeight { get; private set; }

		public CreatureDrop(CampaignWaveData.CreatureDetailInfo creatureInfo, CreatureData creatureData)
		{
			CreatureInfo = creatureInfo;
			CreatureData = creatureData;
			DropItemWeight = CreatureData.DropItemWeight;
			if (CreatureInfo.IsBoss)
			{
				DropItemWeight *= GameSetting.data.BattleField.DropItem_BossExtraWeight;
			}
			DropItemWeight = Mathf.Max(DropItemWeight, 0.01f);
			DropGoldWeight = CreatureData.DropGoldWeight;
			if (CreatureInfo.IsBoss)
			{
				DropGoldWeight *= GameSetting.data.BattleField.DropItem_BossExtraWeight;
			}
			DropGoldWeight = Mathf.Max(DropGoldWeight, 0.01f);
		}
	}

	private class ItemInternal
	{
		public string ItemCode;

		public int Count;
	}

	private IncreaseDic<CreatureKey, CreatureDrop> _dropDic = new IncreaseDic<CreatureKey, CreatureDrop>(new CreatureKey.EqualComparer());

	private float _totalItemWeights;

	private float _totalGoldWeights;

	private int _totalWaveCount;

	private int _totalMaxDropItemCount;

	public void Open(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario, RewardItemInfo[] rewardItems, int dropGold)
	{
		CollectDungeonMonsters(chapterIndex, dungeonIndex, difficulty, isScenario);
		if (_dropDic.Count != 0)
		{
			if (__Extension_Method_ICollection__.COUNT(rewardItems) > 0)
			{
				AssignRewardItems(rewardItems);
			}
			if (dropGold > 0)
			{
				AssignDropGold(dropGold);
			}
		}
	}

	public CreatureDrop GetDrop(bool isBoss, int waveIndex, int waveSlotIndex)
	{
		CreatureDrop vALUE = _dropDic.GETVALUE(new CreatureKey(isBoss, waveIndex, waveSlotIndex), null);
		if (vALUE == null)
		{
			return null;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(vALUE.GoldList) && __Extension_Method_ICollection__.IsNullOrEmpty(vALUE.ItemList))
		{
			return null;
		}
		return vALUE;
	}

	private void AssignRewardItems(RewardItemInfo[] rewardItems)
	{
		List<ItemInternal> list = new List<ItemInternal>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItems); i++)
		{
			RewardItemInfo rewardItemInfo = rewardItems[i];
			list.Add(new ItemInternal
			{
				ItemCode = rewardItemInfo.Code,
				Count = rewardItemInfo.Count
			});
		}
		if (_totalMaxDropItemCount > 0)
		{
			int b = Mathf.FloorToInt((float)_totalMaxDropItemCount / (float)list.Count);
			for (int j = 0; j < list.Count; j++)
			{
				list[j].Count = Mathf.Min(list[j].Count, b);
			}
		}
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(list); k++)
		{
			ItemInternal itemInternal = list[k];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemInternal.ItemCode);
			if (dataByCode == null || dataByCode.Type == ItemType.Hero)
			{
				continue;
			}
			for (int l = 0; l < itemInternal.Count; l++)
			{
				float num = Random.Range(0f, _totalItemWeights);
				for (int m = 0; m < _dropDic.Count; m++)
				{
					CreatureDrop valueByIndex = _dropDic.GetValueByIndex(m);
					if (num > valueByIndex.DropItemWeight && m < _dropDic.Count - 1)
					{
						num -= valueByIndex.DropItemWeight;
						continue;
					}
					valueByIndex.ItemList.Add(dataByCode.Index);
					break;
				}
			}
		}
	}

	private void AssignDropGold(int dropGold)
	{
		int num = Mathf.Max(1, Mathf.Min(dropGold / 10, Mathf.Min(_dropDic.Count / 2, Random.Range(3, 6))));
		int num2 = dropGold;
		for (int i = 0; i < num; i++)
		{
			int num3 = num2;
			if (i < num - 1)
			{
				float num4 = 1f / (float)(num - i);
				num4 = Random.Range(num4 * 0.9f, num4 * 1.1f);
				num3 = (int)((float)num2 * num4);
			}
			if (num3 == 0)
			{
				continue;
			}
			float num5 = Random.Range(0f, _totalGoldWeights);
			for (int j = 0; j < _dropDic.Count; j++)
			{
				CreatureDrop valueByIndex = _dropDic.GetValueByIndex(j);
				if (num5 > valueByIndex.DropGoldWeight && j < _dropDic.Count - 1)
				{
					num5 -= valueByIndex.DropItemWeight;
					continue;
				}
				valueByIndex.GoldList.Add(num3);
				num2 -= num3;
				break;
			}
		}
	}

	private void CollectDungeonMonsters(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario)
	{
		_dropDic.Clear();
		_totalItemWeights = 0f;
		_totalGoldWeights = 0f;
		_totalWaveCount = 0;
		_totalMaxDropItemCount = 0;
		for (int i = 1; i < 100; i++)
		{
			CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, difficulty, scenario, i);
			if (data == null)
			{
				_totalWaveCount = i - 1;
				break;
			}
			List<CampaignWaveData.CreatureDetailInfo> creatureList = data.GetCreatureList();
			for (int j = 0; j < creatureList.Count; j++)
			{
				CampaignWaveData.CreatureDetailInfo creatureDetailInfo = creatureList[j];
				CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(creatureDetailInfo.Index);
				if (creatureDetailInfo != null && data2 != null)
				{
					CreatureKey key = new CreatureKey(creatureDetailInfo.IsBoss, i, creatureDetailInfo.WaveSlotIndex);
					CreatureDrop creatureDrop = new CreatureDrop(creatureDetailInfo, data2);
					_totalItemWeights += creatureDrop.DropItemWeight;
					_totalGoldWeights += creatureDrop.DropGoldWeight;
					_dropDic.Add(key, creatureDrop);
				}
			}
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxDropItemCount");
		if (valueInt > 0)
		{
			_totalMaxDropItemCount = _totalWaveCount * valueInt;
		}
	}
}
