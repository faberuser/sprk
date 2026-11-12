using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class PortalRenewalDataContainer : JitDataContainerListOnly<PortalRenewalData>
{
	public const string TABLENAME = "PortalRenewalTable";

	private JitDictionary<PortalRenewalCategory, List<PortalRenewalData>> _lists_by_Category = new JitDictionary<PortalRenewalCategory, List<PortalRenewalData>>();

	private JitRef<List<PortalRenewalData>> _list_of_SubCategoryIsMenuButton = new JitRef<List<PortalRenewalData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("PortalRenewalTable");

	public PortalRenewalDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_Category);
		base.Packer.RegisterAndBind(_list_of_SubCategoryIsMenuButton);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (PortalRenewalData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.Category != PortalRenewalCategory.NONE)
			{
				_lists_by_Category.JsonLoad_AddToListValue(item.Category, item);
			}
		}
		_list_of_SubCategoryIsMenuButton.JsonLoad_Set((from v in base.BaseList.JsonLoad_GetOrNew()
			where v.Category != PortalRenewalCategory.NONE
			where v.SubCategory == PortalRenewalSubCategory.MENUBUTTON
			select v).ToList());
	}

	public IList<PortalRenewalData> GetData(PortalRenewalCategory category)
	{
		return SprkPetPresentation.Contents(_lists_by_Category.GetOrDefault(category), category);
	}

	public IList<PortalRenewalData> GetCategoryDatas()
	{
		return _list_of_SubCategoryIsMenuButton.Eval();
	}
}
