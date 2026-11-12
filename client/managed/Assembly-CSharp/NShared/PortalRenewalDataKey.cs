using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct PortalRenewalDataKey
{
	[Key(0)]
	public PortalRenewalCategory Category;

	[Key(1)]
	public PortalRenewalSubCategory SubCategory;

	public PortalRenewalDataKey(PortalRenewalCategory category, PortalRenewalSubCategory subCategory)
	{
		Category = category;
		SubCategory = subCategory;
	}

	public bool Equals(PortalRenewalDataKey other)
	{
		if (Category == other.Category)
		{
			return SubCategory == other.SubCategory;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Category, SubCategory);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Category}, {SubCategory})";
	}
}
