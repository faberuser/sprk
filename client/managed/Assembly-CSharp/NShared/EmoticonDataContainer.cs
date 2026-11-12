using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class EmoticonDataContainer : JitDataContainerDefault<int, EmoticonData>
{
	public const string TABLENAME = "EmoticonTable";

	private JitDictionary<int, List<EmoticonData>> _emoticonDicByTabIndex = new JitDictionary<int, List<EmoticonData>>();

	private JitDictionary<string, EmoticonData> _emoticonDicByKeyword = new JitDictionary<string, EmoticonData>();

	public override TablePathDefine PathDefine => new TablePathDefine("EmoticonTable");

	public EmoticonDataContainer()
	{
		base.Packer.RegisterAndBind(_emoticonDicByTabIndex);
		base.Packer.RegisterAndBind(_emoticonDicByKeyword);
	}

	protected override void OnEndLoadFromJsonData()
	{
		List<EmoticonData> list = base.BaseList.JsonLoad_GetOrNew().ToList();
		list.Sort((EmoticonData lhs, EmoticonData rhs) => lhs.Index.CompareTo(rhs.Index));
		foreach (EmoticonData item in list)
		{
			_emoticonDicByKeyword.JsonLoad_Get()[item.Keyword] = item;
			_emoticonDicByTabIndex.JsonLoad_AddToListValue(item.TabIndex, item);
		}
	}

	public EmoticonData GetDataByKeyword(string keyword)
	{
		if (keyword == null || keyword.Length == 0)
		{
			return null;
		}
		EmoticonData value = null;
		_emoticonDicByKeyword.TryGetValue(keyword, out value);
		return value;
	}

	public IList<EmoticonData> GetDatasByTabIndex(int index)
	{
		return _emoticonDicByTabIndex.GetOrDefault(index);
	}
}
