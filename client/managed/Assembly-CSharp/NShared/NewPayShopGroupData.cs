using System;
using System.Runtime.CompilerServices;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NewPayShopGroupData : IDataKeySelector<string>, IComparable<NewPayShopGroupData>
{
	[CompilerGenerated]
	private bool _003CIsEnable_003Ek__BackingField;

	[Key(0)]
	public string PayShopCategoryType { get; set; }

	[Key(1)]
	public string CategoryName { get; set; }

	[Key(2)]
	public string ParentCategory { get; set; }

	[Key(3)]
	public bool IsCustomize { get; set; }

	[Key(4)]
	public int Sort { get; set; }

	[Key(5)]
	public bool IsEnable
	{
		[CompilerGenerated]
		get
		{
			if (PayShopCategoryType == "PetGacha")
			{
				return true;
			}
			return _003CIsEnable_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			_003CIsEnable_003Ek__BackingField = value;
		}
	}

	[Key(6)]
	public CurrencyType LinkCurrency { get; set; }

	[Key(7)]
	public EGroupEnableConditionType EnableCondition { get; set; }

	[Key(8)]
	public NewPayShopCategoryType EPayShopCategoryType { get; set; }

	[Key(9)]
	public NewPayShopCategoryType EParentCategory { get; set; }

	string IDataKeySelector<string>.Key => PayShopCategoryType;

	int IComparable<NewPayShopGroupData>.CompareTo(NewPayShopGroupData other)
	{
		return Compare(this, other);
	}

	public static int Compare(NewPayShopGroupData a, NewPayShopGroupData b)
	{
		if (!GameRoot.isValidInstance || GameRoot.Instance.NewPayShopGroupDataContainer == null)
		{
			return a.Sort.CompareTo(b.Sort);
		}
		int result = 0;
		if (string.IsNullOrEmpty(a.ParentCategory) && string.IsNullOrEmpty(b.ParentCategory))
		{
			result = a.Sort.CompareTo(b.Sort);
		}
		else if (string.IsNullOrEmpty(a.ParentCategory) && !string.IsNullOrEmpty(b.ParentCategory))
		{
			if (a.PayShopCategoryType == b.ParentCategory)
			{
				return -1;
			}
			NewPayShopGroupData data = GameRoot.Instance.NewPayShopGroupDataContainer.GetData(b.ParentCategory);
			result = a.Sort.CompareTo(data.Sort);
		}
		else if (!string.IsNullOrEmpty(a.ParentCategory) && string.IsNullOrEmpty(b.ParentCategory))
		{
			if (b.PayShopCategoryType == a.ParentCategory)
			{
				return 1;
			}
			result = GameRoot.Instance.NewPayShopGroupDataContainer.GetData(a.ParentCategory).Sort.CompareTo(b.Sort);
		}
		else if (!string.IsNullOrEmpty(a.ParentCategory) && !string.IsNullOrEmpty(b.ParentCategory))
		{
			if (a.ParentCategory == b.ParentCategory)
			{
				result = a.Sort.CompareTo(b.Sort);
			}
			else
			{
				NewPayShopGroupData data2 = GameRoot.Instance.NewPayShopGroupDataContainer.GetData(a.ParentCategory);
				NewPayShopGroupData data3 = GameRoot.Instance.NewPayShopGroupDataContainer.GetData(b.ParentCategory);
				result = data2.Sort.CompareTo(data3.Sort);
			}
		}
		return result;
	}
}
