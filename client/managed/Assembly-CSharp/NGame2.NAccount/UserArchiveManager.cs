using System;
using System.Collections.Generic;
using System.Linq;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserArchiveManager : NShared.UserArchiveManager
{
	public Dictionary<CreatureTagType, List<int>> _heroArchiveDic = new Dictionary<CreatureTagType, List<int>>();

	public Dictionary<ArchiveType, List<int>> _monsterArchiveDic = new Dictionary<ArchiveType, List<int>>();

	public Dictionary<int, int> _itemArchiveAwakeDic = new Dictionary<int, int>();

	public Dictionary<ArchiveSubType, List<int>> _itemArchiveDic = new Dictionary<ArchiveSubType, List<int>>();

	public Dictionary<CreatureGrade, List<int>> _petArchiveDic = new Dictionary<CreatureGrade, List<int>>();

	public Dictionary<CreatureTagType, List<int>> _itemArchiveUniqueWeaponDic = new Dictionary<CreatureTagType, List<int>>();

	public Dictionary<CreatureTagType, List<int>> _itemArchiveUniqueTreasureDic = new Dictionary<CreatureTagType, List<int>>();

	public Dictionary<EquipPartType, List<int>> _itemArchiveEquipPartDic = new Dictionary<EquipPartType, List<int>>();

	public Dictionary<byte, List<int>> _itemArchiveEquipGradeDic = new Dictionary<byte, List<int>>();

	public Dictionary<ArchiveSubType2, List<int>> _itemArchiveNormalDic = new Dictionary<ArchiveSubType2, List<int>>();

	public List<int> _itemArdhiveArtifactList = new List<int>();

	public void Apply(PlayerArchiveInfo resultInfo)
	{
		if (resultInfo == null || resultInfo.ArchiveValue == null)
		{
			return;
		}
		Add(resultInfo);
		int archiveMainIndex = resultInfo.ArchiveMainIndex;
		int archiveSubIndex = resultInfo.ArchiveSubIndex;
		PlayerArchiveData data = GameRoot.Instance.PlayerArchiveDataContainer.GetData(archiveMainIndex, archiveSubIndex);
		if (data == null)
		{
			return;
		}
		switch (data.ArchiveMainType)
		{
		case ArchiveMainType.Hero:
		{
			CreatureTagType archiveSubIndex4 = (CreatureTagType)resultInfo.ArchiveSubIndex;
			string[] array5 = resultInfo.ArchiveValue.Split(',');
			if (!_heroArchiveDic.ContainsKey(archiveSubIndex4))
			{
				_heroArchiveDic.Add(archiveSubIndex4, new List<int>());
			}
			string[] array2 = array5;
			foreach (string text5 in array2)
			{
				if (!String_Extension_Method.IsNullOrEmpty(text5))
				{
					int item4 = Convert.ToInt32(text5);
					if (!_heroArchiveDic[archiveSubIndex4].Contains(item4))
					{
						_heroArchiveDic[archiveSubIndex4].Add(item4);
					}
				}
			}
			break;
		}
		case ArchiveMainType.Monster:
		{
			ArchiveType archiveSubIndex3 = (ArchiveType)resultInfo.ArchiveSubIndex;
			string[] array3 = resultInfo.ArchiveValue.Split(',');
			if (!_monsterArchiveDic.ContainsKey(archiveSubIndex3))
			{
				_monsterArchiveDic.Add(archiveSubIndex3, new List<int>());
			}
			string[] array2 = array3;
			foreach (string text2 in array2)
			{
				if (!String_Extension_Method.IsNullOrEmpty(text2))
				{
					int num = Convert.ToInt32(text2);
					if (GameRoot.Instance.CreatureDataContainer.GetData(num) != null && !_monsterArchiveDic[archiveSubIndex3].Contains(num))
					{
						_monsterArchiveDic[archiveSubIndex3].Add(num);
					}
				}
			}
			break;
		}
		case ArchiveMainType.Item:
		{
			ArchiveSubType archiveSubType = data.ArchiveSubType;
			string[] array4 = resultInfo.ArchiveValue.Split(',');
			if (data.ConditionValue1.First() == ItemType.Equip)
			{
				List<int> list = new List<int>();
				List<int> list2 = new List<int>();
				for (int j = 0; j < array4.Length; j++)
				{
					string text3 = array4[j];
					int num2 = Vespa_String_Extension_Method.TOINT(text3, -1);
					if (num2 == -1)
					{
						VespaDebug.LogError($"PlayerArchiveData : mainIndex {data.MainIndex} subIndex {data.SubIndex} value {text3}");
					}
					else if (j % 2 == 0)
					{
						list.Add(num2);
					}
					else
					{
						list2.Add(num2);
					}
				}
				if (list.Count != list2.Count)
				{
					VespaDebug.LogError("UserArchiveManager Item : Invalid ListCount");
					break;
				}
				if (!_itemArchiveDic.ContainsKey(archiveSubType))
				{
					_itemArchiveDic.Add(archiveSubType, new List<int>());
				}
				for (int k = 0; k < list.Count; k++)
				{
					int num3 = list[k];
					int value = list2[k];
					if (!_itemArchiveDic[archiveSubType].Contains(num3))
					{
						_itemArchiveDic[archiveSubType].Add(num3);
					}
					if (_itemArchiveAwakeDic.ContainsKey(num3))
					{
						_itemArchiveAwakeDic[num3] = value;
					}
					else
					{
						_itemArchiveAwakeDic.Add(num3, value);
					}
				}
				break;
			}
			List<int> list3 = new List<int>();
			foreach (string text4 in array4)
			{
				if (!String_Extension_Method.IsNullOrEmpty(text4))
				{
					int item2 = Convert.ToInt32(text4);
					list3.Add(item2);
				}
			}
			if (!_itemArchiveDic.ContainsKey(archiveSubType))
			{
				_itemArchiveDic.Add(archiveSubType, new List<int>());
			}
			for (int m = 0; m < list3.Count; m++)
			{
				int item3 = list3[m];
				if (!_itemArchiveDic[archiveSubType].Contains(item3))
				{
					_itemArchiveDic[archiveSubType].Add(item3);
				}
			}
			break;
		}
		case ArchiveMainType.Pet:
		{
			CreatureGrade archiveSubIndex2 = (CreatureGrade)resultInfo.ArchiveSubIndex;
			string[] array = resultInfo.ArchiveValue.Split(',');
			if (!_petArchiveDic.ContainsKey(archiveSubIndex2))
			{
				_petArchiveDic.Add(archiveSubIndex2, new List<int>());
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!String_Extension_Method.IsNullOrEmpty(text))
				{
					int item = Convert.ToInt32(text);
					if (!_petArchiveDic[archiveSubIndex2].Contains(item))
					{
						_petArchiveDic[archiveSubIndex2].Add(item);
					}
				}
			}
			break;
		}
		}
	}

	public void Apply(PlayerArchiveInfo[] resultInfos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(resultInfos); i++)
		{
			Apply(resultInfos[i]);
		}
	}

	public int GetHeroArchiveCount(CreatureTagType creatureTagType)
	{
		// The server sends no hero archive entries, so owned heroes also count
		// (the same heroes the Hero Index shows in color).
		HashSet<int> collected = new HashSet<int>();
		foreach (KeyValuePair<CreatureTagType, List<int>> item in _heroArchiveDic)
		{
			if (creatureTagType == CreatureTagType.None || item.Key == creatureTagType)
			{
				collected.UnionWith(item.Value);
			}
		}
		UserHeroManager heroManager = GlobalAutoBehaviour<AccountManager>.instance.HeroManager;
		if (heroManager != null)
		{
			foreach (CachedHeroInfo hero in heroManager)
			{
				CreatureData creatureData = (hero == null) ? null : GameRoot.Instance.CreatureDataContainer.GetData(hero.HeroIndex);
				if (creatureData != null && creatureData.OpenType != CreatureOpenType.None && creatureData.OpenType != CreatureOpenType.CommingSoon && (creatureTagType == CreatureTagType.None || creatureData.TagType == creatureTagType))
				{
					collected.Add(hero.HeroIndex);
				}
			}
		}
		return collected.Count;
	}

	public int GetSubArchiveCount(ArchiveType archiveType)
	{
		if (archiveType == ArchiveType.None)
		{
			int num = 0;
			{
				foreach (KeyValuePair<ArchiveType, List<int>> item in _monsterArchiveDic)
				{
					num += item.Value.Count;
				}
				return num;
			}
		}
		int num2 = 0;
		foreach (KeyValuePair<ArchiveType, List<int>> item2 in _monsterArchiveDic)
		{
			if (item2.Key == archiveType)
			{
				num2 += item2.Value.Count;
			}
		}
		return num2;
	}

	public int GetPetArchiveCount(CreatureGrade grade)
	{
		// The server sends no pet archive entries, so collected pets also come
		// from the pets the player owns (the same rows the book shows in color).
		HashSet<int> collected = new HashSet<int>();
		foreach (KeyValuePair<CreatureGrade, List<int>> item in _petArchiveDic)
		{
			if (grade == CreatureGrade.None || item.Key == grade)
			{
				collected.UnionWith(item.Value);
			}
		}
		UserPetManager petManager = GlobalAutoBehaviour<AccountManager>.instance.PetManager;
		if (petManager != null)
		{
			foreach (PetInfo pet in petManager)
			{
				CreatureData creatureData = pet?.CreatureData;
				if (creatureData != null && creatureData.Race == CreatureRaceType.Pet && (grade == CreatureGrade.None || creatureData.CreatureGrade == grade))
				{
					collected.Add(pet.PetIndex);
				}
			}
		}
		return collected.Count;
	}

	public List<int> GetPetList(CreatureGrade grade)
	{
		if (_petArchiveDic.ContainsKey(grade))
		{
			return _petArchiveDic[grade];
		}
		return null;
	}

	public List<int> GetMonsterList(ArchiveType archiveType)
	{
		if (_monsterArchiveDic == null)
		{
			return null;
		}
		foreach (KeyValuePair<ArchiveType, List<int>> item in _monsterArchiveDic)
		{
			if (item.Key == archiveType)
			{
				return item.Value;
			}
		}
		return null;
	}

	public bool isClearMonster(ArchiveType archiveType, int heroIndex)
	{
		if (_monsterArchiveDic == null)
		{
			return false;
		}
		if (!_monsterArchiveDic.ContainsKey(archiveType))
		{
			return false;
		}
		return _monsterArchiveDic[archiveType].Contains(heroIndex);
	}

	public int GetUniqueItemArchiveCount(ArchiveSubType subType, CreatureTagType tagType = CreatureTagType.None)
	{
		switch (subType)
		{
		case ArchiveSubType.UniqueWeapon:
			if (tagType == CreatureTagType.None)
			{
				int num2 = 0;
				{
					foreach (KeyValuePair<CreatureTagType, List<int>> item in _itemArchiveUniqueWeaponDic)
					{
						num2 += item.Value.Count;
					}
					return num2;
				}
			}
			return _itemArchiveUniqueWeaponDic[tagType].Count;
		case ArchiveSubType.UniqueTreasure:
			if (tagType == CreatureTagType.None)
			{
				int num = 0;
				{
					foreach (KeyValuePair<CreatureTagType, List<int>> item2 in _itemArchiveUniqueTreasureDic)
					{
						num += item2.Value.Count;
					}
					return num;
				}
			}
			return _itemArchiveUniqueTreasureDic[tagType].Count;
		case ArchiveSubType.Artifact:
			return _itemArdhiveArtifactList.Count;
		default:
			return 0;
		}
	}

	public int GetEquipItemArchiveCount(EquipPartType partType, byte grade)
	{
		List<int> list = _itemArchiveEquipPartDic[partType];
		List<int> list2 = _itemArchiveEquipGradeDic[grade];
		int num = 0;
		foreach (int item in list)
		{
			if (list2.Contains(item))
			{
				num++;
			}
		}
		return num;
	}

	public int GetNormalItemArchiveCount(ArchiveSubType2 subType2)
	{
		if (!_itemArchiveNormalDic.ContainsKey(subType2))
		{
			return 0;
		}
		return _itemArchiveNormalDic[subType2].Count;
	}

	public bool IsContainItem(int itemIndex)
	{
		if (OwnedItemStars().ContainsKey(itemIndex))
		{
			return true;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_itemArchiveDic))
		{
			return false;
		}
		foreach (KeyValuePair<ArchiveSubType, List<int>> item in _itemArchiveDic)
		{
			if (item.Value.Contains(itemIndex))
			{
				return true;
			}
		}
		return false;
	}

	public int GetItemAwake(int itemInex)
	{
		int value;
		int num = (_itemArchiveAwakeDic.TryGetValue(itemInex, out value) ? value : 0);
		int value2;
		if (OwnedItemStars().TryGetValue(itemInex, out value2) && value2 > num)
		{
			num = value2;
		}
		return num;
	}

	public int GetItemCount()
	{
		HashSet<int> collected = new HashSet<int>();
		foreach (KeyValuePair<ArchiveSubType, List<int>> item in _itemArchiveDic)
		{
			collected.UnionWith(item.Value);
		}
		foreach (int key in OwnedItemStars().Keys)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(key);
			if (data != null && data.IsArchive)
			{
				collected.Add(key);
			}
		}
		return collected.Count;
	}

	[NonSerialized]
	private Dictionary<int, int> _ownedItemStars;

	[NonSerialized]
	private int _ownedItemStarsFrame = -1;

	// The server sends no item archive entries, so every item the account holds
	// (inventory and equipment, including equipped gear) counts as collected,
	// with the highest star among its copies. Rebuilt at most once per frame,
	// since the Item Index asks per icon.
	private Dictionary<int, int> OwnedItemStars()
	{
		int frameCount = UnityEngine.Time.frameCount;
		if (_ownedItemStars != null && _ownedItemStarsFrame == frameCount)
		{
			return _ownedItemStars;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (instance.EquipItemManager != null)
		{
			foreach (EquipItemInfo equip in instance.EquipItemManager)
			{
				if (equip != null && equip.ItemIndex > 0)
				{
					int value;
					dictionary[equip.ItemIndex] = (dictionary.TryGetValue(equip.ItemIndex, out value) ? System.Math.Max(value, equip.Star) : equip.Star);
				}
			}
		}
		if (instance.ItemManager != null)
		{
			foreach (ItemInfo item in instance.ItemManager)
			{
				if (item != null && item.ItemIndex > 0 && item.Count > 0 && !dictionary.ContainsKey(item.ItemIndex))
				{
					dictionary[item.ItemIndex] = 0;
				}
			}
		}
		_ownedItemStars = dictionary;
		_ownedItemStarsFrame = frameCount;
		return dictionary;
	}
}
