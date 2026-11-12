using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class HairCostumeDataContainer : JitDataContainerDefault<int, HairCostumeData>
{
	public const string TABLENAME = "HairCostumeTable";

	private JitDictionary<int, List<HairCostumeData>> _lists_by_HeroIndex = new JitDictionary<int, List<HairCostumeData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("HairCostumeTable");

	public HairCostumeDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_HeroIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (HairCostumeData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_HeroIndex.JsonLoad_AddToListValue(item.HeroIndex, item);
		}
	}

	public IList<HairCostumeData> GetDataListByHeroIndex(int heroIndex)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_lists_by_HeroIndex.GetOrDefault(heroIndex));
	}

	public HairCostumeData GetDataByHeroStar(int heroIndex, int star)
	{
		foreach (HairCostumeData item in __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_lists_by_HeroIndex.GetOrDefault(heroIndex)))
		{
			if (item.Star == star)
			{
				return item;
			}
		}
		return null;
	}
}
