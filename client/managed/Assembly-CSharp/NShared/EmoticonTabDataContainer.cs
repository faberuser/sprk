using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EmoticonTabDataContainer : JitDataContainerDefault<int, EmoticonTabData>
{
	public const string TABLENAME = "EmoticonTabTable";

	private JitRef<List<EmoticonTabData>> _sortedValueList = new JitRef<List<EmoticonTabData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("EmoticonTabTable");

	public EmoticonTabDataContainer()
	{
		base.Packer.RegisterAndBind(_sortedValueList);
	}

	protected override void OnEndLoadFromJsonData()
	{
		_sortedValueList.JsonLoad_GetOrNew().AddRange(base.BaseList.JsonLoad_GetOrNew());
		_sortedValueList.JsonLoad_GetOrNew().Sort((EmoticonTabData lhs, EmoticonTabData rhs) => lhs.ShowIndex.CompareTo(rhs.ShowIndex));
	}

	public IList<EmoticonTabData> GetTabList()
	{
		return _sortedValueList.Eval();
	}
}
