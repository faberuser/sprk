using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class AccessoryCostumeDataContainer : JitDataContainerDefault<int, AccessoryCostumeData>
{
	public const string TABLENAME = "AccessoryCostumeTable";

	private JitRef<List<AccessoryCostumeData>> _newCostumeList = new JitRef<List<AccessoryCostumeData>>();

	private JitDictionary<string, List<AccessoryCostumeData>> _lists_by_Category = new JitDictionary<string, List<AccessoryCostumeData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("AccessoryCostumeTable");

	public AccessoryCostumeDataContainer()
	{
		base.Packer.RegisterAndBind(_newCostumeList);
		base.Packer.RegisterAndBind(_lists_by_Category);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (AccessoryCostumeData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.New > 0)
			{
				_newCostumeList.JsonLoad_GetOrNew().Add(item);
			}
			if (!String_Extension_Method.IsNullOrEmpty(item.Category))
			{
				_lists_by_Category.JsonLoad_AddToListValue(item.Category, item);
			}
		}
	}

	public bool NewCostumeExists()
	{
		return !__Extension_Method_ICollection__.IsNullOrEmpty(_newCostumeList.Eval());
	}

	public IList<AccessoryCostumeData> GetDataByCategory(string category)
	{
		if (category == string.Empty)
		{
			return GetAllData();
		}
		if (category == "New")
		{
			return _newCostumeList.Eval();
		}
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_lists_by_Category.GetOrDefault(category));
	}
}
