using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class HelpListDataContainer : JitDataContainerDefault<HelpListDataKey, HelpListData>
{
	public const string TABLENAME = "HelpListTable";

	private JitDictionary<string, List<HelpListData>> _lists_by_Code = new JitDictionary<string, List<HelpListData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("HelpListTable");

	public HelpListDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_Code);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (HelpListData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!String_Extension_Method.IsNullOrEmpty(item.Code))
			{
				_lists_by_Code.JsonLoad_AddToListValue(item.Code, item);
			}
		}
	}

	public int GetMaxPageCountByCode(string code)
	{
		if (string.IsNullOrEmpty(code))
		{
			return 0;
		}
		return _lists_by_Code.GetOrDefault(code)?.Count ?? 0;
	}
}
