using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class ValanceCraftDataContainer : JitDataContainerDefault<ValanceCraftDataKey2, ValanceCraftData>
{
	public const string TABLENAME = "ValanceCraftTable";

	private JitRef<List<ValanceCraftData>> _list_of_NoticeCheck = new JitRef<List<ValanceCraftData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ValanceCraftTable");

	public ValanceCraftDataContainer()
	{
		base.Packer.RegisterAndBind(_list_of_NoticeCheck);
	}

	protected override void OnEndLoadFromJsonData()
	{
		_list_of_NoticeCheck.JsonLoad_Set((from v in base.BaseList.JsonLoad_GetOrNew()
			where v.NoticeCheck
			select v).ToList());
	}

	public ValanceCraftData GetData(ValanceCraftDataKey key)
	{
		if (key == null)
		{
			return null;
		}
		return GetData(new ValanceCraftDataKey2(key.SetType, key.CreatureTagType, key.EquipPartType, key.EquipItemSubType));
	}

	public ValanceCraftData GetData(IValanceEquipFilterInfo key)
	{
		if (key == null)
		{
			return null;
		}
		EquipItemSetGroupType setType = key.SetType;
		CreatureTagType tagType = (key.IsCreatureTagTypeArray ? key.CreatureTagTypes[0] : key.CreatureTagType);
		EquipPartType equipPartType = key.EquipPartType;
		EquipItemSubType itemSubType = ((key.EquipPartType == EquipPartType.Accessory) ? key.EquipItemSubType : EquipItemSubType.None);
		return GetData(new ValanceCraftDataKey2(setType, tagType, equipPartType, itemSubType));
	}

	public IList<ValanceCraftData> GetNoticeCheckList()
	{
		return _list_of_NoticeCheck.Eval();
	}
}
