using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class BoosterItemDataContainer : SubItemDataContainer<BoosterItemData>
{
	public const string TABLENAME = "BoosterItemTable";

	private JitDictionary<int, List<BoosterItemData>> _dicByBoosterGroupIndex = new JitDictionary<int, List<BoosterItemData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("BoosterItemTable");

	public BoosterItemDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByBoosterGroupIndex);
	}

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.Booster };
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<string, BoosterItemData> item in base.BaseDic.JsonLoad_Get())
		{
			_dicByBoosterGroupIndex.JsonLoad_AddToListValue(item.Value.BoosterItemIndex, item.Value);
		}
	}

	public BoosterItemData GetBoosterDataByGroupIndex(int BoosterGroupIndex)
	{
		return _dicByBoosterGroupIndex.GetOrDefault(BoosterGroupIndex)?[0];
	}
}
