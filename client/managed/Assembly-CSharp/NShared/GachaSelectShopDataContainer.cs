using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GachaSelectShopDataContainer : JitDataContainerDefault<int, GachaSelectShopData>
{
	public const string TABLENAME = "GachaSelectShopTable";

	private JitDictionary<string, List<GachaSelectShopData>> _lists_by_CategoryGroup = new JitDictionary<string, List<GachaSelectShopData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("GachaSelectShopTable");

	public GachaSelectShopDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_CategoryGroup);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (GachaSelectShopData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_CategoryGroup.JsonLoad_AddToListValue(item.CategoryGroup, item);
		}
	}

	public IList<GachaSelectShopData> GetEqualCategoryList(string categoryGroup)
	{
		return _lists_by_CategoryGroup.GetOrDefault(categoryGroup);
	}
}
