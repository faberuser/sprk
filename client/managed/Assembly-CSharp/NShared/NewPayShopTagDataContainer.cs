using System;
using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class NewPayShopTagDataContainer : JitDataContainerDefault<int, NewPayShopTagData>
{
	public const string TABLENAME = "NewPayShopTagTable";

	private const string DEFAULT_CATEGORY = "All";

	private JitRef<HashSet<string>> _set_of_CategoryType = new JitRef<HashSet<string>>();

	public override TablePathDefine PathDefine => new TablePathDefine("NewPayShopTagTable");

	public NewPayShopTagDataContainer()
	{
		base.Packer.RegisterAndBind(_set_of_CategoryType);
	}

	protected override void OnEndLoadFromJsonData()
	{
		IEnumerable<string> values = (from v in base.BaseList.JsonLoad_GetOrNew()
			where !String_Extension_Method.IsNullOrEmpty(v.PayShopCategoryType)
			select v.PayShopCategoryType).Distinct();
		__Extension_Method_ICollection__.ADDRANGE(_set_of_CategoryType.JsonLoad_GetOrNew(), values);
	}

	public NewPayShopTagData[] GetTags(string category)
	{
		SprkShopTags.Refresh(this);
		NewPayShopTagData[] array = (from v in GetAllData()
			where v.PayShopCategoryType == category
			select v).ToArray();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(array))
		{
			return array;
		}
		return (from v in GetAllData()
			where v.PayShopCategoryType.Equals("All", StringComparison.OrdinalIgnoreCase)
			select v).ToArray();
	}

	public bool IsEqualCategory(string category1, string category2)
	{
		string obj = (_set_of_CategoryType.Eval().Contains(category1) ? category1 : "All");
		string text = (_set_of_CategoryType.Eval().Contains(category2) ? category2 : "All");
		return obj == text;
	}

	public NewPayShopTagData[] GetTags(PayShopIndexType indexType, int productIndex)
	{
		return (from v in GetAllData()
			where v.IndexType == indexType
			where v.Indices != null
			where TypeIterator.Contains(v.Indices, productIndex)
			select v).ToArray();
	}
}
