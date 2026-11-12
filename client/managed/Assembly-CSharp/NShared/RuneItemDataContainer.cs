using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class RuneItemDataContainer : SubItemDataContainer<RuneItemData>
{
	public const string TABLENAME = "RuneItemTable";

	private JitDictionary<string, List<RuneItemData>> _dicByTag = new JitDictionary<string, List<RuneItemData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("RuneItemTable");

	public RuneItemDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByTag);
	}

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.Rune };
	}

	public IList<RuneItemData> GetDatasByTag(string tag)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_dicByTag.GetOrDefault(tag));
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<string, RuneItemData> item in base.BaseDic.JsonLoad_Get())
		{
			UpdateEquipPartTypeFlag(item.Value);
		}
		foreach (KeyValuePair<string, RuneItemData> item2 in base.BaseDic.JsonLoad_Get())
		{
			string[] tag = item2.Value.Tag;
			if (tag != null)
			{
				foreach (string key in tag)
				{
					_dicByTag.JsonLoad_AddToListValue(key, item2.Value);
				}
			}
		}
	}

	private static void UpdateEquipPartTypeFlag(RuneItemData value)
	{
		EquipPartTypeFlag equipPartTypeFlag = EquipPartTypeFlag.None;
		EquipPartType[] partTypes = value.PartTypes;
		for (int i = 0; i < partTypes.Length; i++)
		{
			switch (partTypes[i])
			{
			case EquipPartType.Weapon:
				equipPartTypeFlag |= EquipPartTypeFlag.Weapon;
				break;
			case EquipPartType.Armor:
				equipPartTypeFlag |= EquipPartTypeFlag.Armor;
				break;
			case EquipPartType.Accessory:
				equipPartTypeFlag |= EquipPartTypeFlag.Accessory;
				break;
			case EquipPartType.SecondGear:
				equipPartTypeFlag |= EquipPartTypeFlag.SecondGear;
				break;
			case EquipPartType.Artifact:
				equipPartTypeFlag |= EquipPartTypeFlag.Artifact;
				break;
			case EquipPartType.Orb:
				equipPartTypeFlag |= EquipPartTypeFlag.Orb;
				break;
			case EquipPartType.Treasure:
				equipPartTypeFlag |= EquipPartTypeFlag.Treasure;
				break;
			}
		}
		value.PartTypeFlag = equipPartTypeFlag;
	}
}
