using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EquipItemDataContainer : SubItemDataContainer<EquipItemData>
{
	public const string TABLENAME = "EquipItemTable";

	private JitDictionaryWithKey<string, EquipItemData> _dicForAllClassTag = new JitDictionaryWithKey<string, EquipItemData>();

	private JitDictionary<int, EquipItemData> _dicForOpenSoulWeapon = new JitDictionary<int, EquipItemData>();

	private JitDictionary<int, List<EquipItemData>> _dicByDetailIndex = new JitDictionary<int, List<EquipItemData>>();

	private JitDictionary<EquipPartType, List<EquipItemData>> _dicByEquipPart = new JitDictionary<EquipPartType, List<EquipItemData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("EquipItemTable");

	public EquipItemDataContainer()
	{
		base.Packer.RegisterAndBind(_dicForAllClassTag);
		base.Packer.RegisterAndBind(_dicForOpenSoulWeapon);
		base.Packer.RegisterAndBind(_dicByDetailIndex);
		base.Packer.RegisterAndBind(_dicByEquipPart);
	}

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[2]
		{
			ItemType.Equip,
			ItemType.PunishmentRune
		};
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<string, EquipItemData> item in base.BaseDic.JsonLoad_Get())
		{
			if (HasAllClassTag(item.Value))
			{
				_dicForAllClassTag.JsonLoad_Get().Add(item.Key, item.Value);
			}
		}
		foreach (KeyValuePair<string, EquipItemData> item2 in base.BaseDic.JsonLoad_Get())
		{
			if (item2.Value.IsOpenSoulWeapon)
			{
				int key = item2.Value.CreatureIndex[0];
				_dicForOpenSoulWeapon.JsonLoad_Get().Add(key, item2.Value);
			}
		}
		foreach (KeyValuePair<string, EquipItemData> item3 in base.BaseDic.JsonLoad_Get())
		{
			_dicByDetailIndex.JsonLoad_AddToListValue(item3.Value.DetailIndex, item3.Value);
			_dicByEquipPart.JsonLoad_AddToListValue(item3.Value.PartType, item3.Value);
		}
	}

	private static bool HasAllClassTag(EquipItemData value)
	{
		if (value.TagType == null)
		{
			return false;
		}
		if (value.TagType.Length < 7)
		{
			return false;
		}
		for (CreatureTagType creatureTagType = CreatureTagType.Warrior; creatureTagType < CreatureTagType.End; creatureTagType++)
		{
			bool flag = false;
			for (int i = 0; i < value.TagType.Length; i++)
			{
				if (value.TagType[i] == creatureTagType)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public IList<EquipItemData> GetDataByEquipPartType(EquipPartType partType)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_dicByEquipPart.GetOrDefault(partType));
	}

	public IList<EquipItemData> GetDataByDetailIndex(int detailIndex)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_dicByDetailIndex.GetOrDefault(detailIndex));
	}

	public EquipItemData GetDataByOpenSoulWeapon(int heroIndex)
	{
		return _dicForOpenSoulWeapon.GetOrDefault(heroIndex);
	}

	public bool ContainsAllClassTag(string EquipCode)
	{
		EquipItemData value;
		return _dicForAllClassTag.TryGetValue(EquipCode, out value);
	}

	public bool IsEnhanceStone(string code)
	{
		if (string.IsNullOrEmpty(code))
		{
			return false;
		}
		EquipItemData data = GetData(code);
		if (data == null)
		{
			return false;
		}
		if (data.PartType == EquipPartType.EnhanceStone)
		{
			return true;
		}
		return false;
	}
}
