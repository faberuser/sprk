using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class NewPayShopGroupDataContainer : JitDataContainerDefault<string, NewPayShopGroupData>
{
	public const string TABLENAME = "NewPayShopGroupTable";

	private static Dictionary<string, NewPayShopCategoryType> _enmuLookup = new Dictionary<string, NewPayShopCategoryType>();

	public override TablePathDefine PathDefine => new TablePathDefine("NewPayShopGroupTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, NewPayShopGroupData value)
	{
		value.EParentCategory = StringToCategoryType(value.ParentCategory);
		value.EPayShopCategoryType = StringToCategoryType(value.PayShopCategoryType);
	}

	public static NewPayShopCategoryType ParseEnum(string enumString)
	{
		if (string.IsNullOrEmpty(enumString))
		{
			return NewPayShopCategoryType.None;
		}
		try
		{
			return (NewPayShopCategoryType)Enum.Parse(typeof(NewPayShopCategoryType), enumString, ignoreCase: true);
		}
		catch
		{
			return NewPayShopCategoryType.None;
		}
	}

	public static NewPayShopCategoryType StringToCategoryType(string enumString)
	{
		if (string.IsNullOrEmpty(enumString))
		{
			return NewPayShopCategoryType.None;
		}
		if (!_enmuLookup.ContainsKey(enumString))
		{
			NewPayShopCategoryType value = ParseEnum(enumString);
			_enmuLookup.Add(enumString, value);
		}
		return _enmuLookup[enumString];
	}

	public bool IsParentAndHasChild(NewPayShopGroupData parentValue)
	{
		if (!string.IsNullOrEmpty(parentValue.ParentCategory))
		{
			return false;
		}
		return GetAllData().Any((NewPayShopGroupData v) => v.ParentCategory == parentValue.PayShopCategoryType);
	}

	public NewPayShopGroupData[] GetDatasWithSameParent(string parentCategory)
	{
		return (from v in GetAllData()
			where v.ParentCategory == parentCategory
			select v).ToArray();
	}

	public NewPayShopGroupData GetFirstDataWithSameParent(string parentCategory)
	{
		NewPayShopGroupData[] datasWithSameParent = GetDatasWithSameParent(parentCategory);
		if (datasWithSameParent == null || datasWithSameParent.Length == 0)
		{
			return null;
		}
		Array.Sort(datasWithSameParent, delegate(NewPayShopGroupData a, NewPayShopGroupData b)
		{
			int num = 0;
			num = a.Sort.CompareTo(b.Sort);
			return (num != 0) ? num : a.PayShopCategoryType.CompareTo(b.PayShopCategoryType);
		});
		return datasWithSameParent[0];
	}

	public bool IsOpenCategoryWithCurrency(CurrencyType currency)
	{
		return GetAllData().Any((NewPayShopGroupData v) => v.IsEnable && v.LinkCurrency == currency);
	}
}
