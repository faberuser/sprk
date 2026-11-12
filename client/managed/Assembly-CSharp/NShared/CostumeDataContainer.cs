using System.Collections.Generic;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class CostumeDataContainer : JitDataContainerDefault<int, CostumeData>
{
	[MessagePackObject(false)]
	[JitKey]
	public struct HeroAndStarKey
	{
		[Key(0)]
		public int Index;

		[Key(1)]
		public int Star;

		public HeroAndStarKey(int index, int star)
		{
			Index = index;
			Star = star;
		}

		public bool Equals(HeroAndStarKey other)
		{
			if (Index == other.Index)
			{
				return Star == other.Star;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JitOption.BuildHash(Index, Star);
		}

		public override string ToString()
		{
			return $"Index={Index} Star={Star}";
		}
	}

	public const string TABLENAME = "CostumeTable";

	private JitDictionary<HeroAndStarKey, List<CostumeData>> _datas_by_HeroStar = new JitDictionary<HeroAndStarKey, List<CostumeData>>();

	private JitDictionary<string, List<CostumeData>> _datas_by_Group = new JitDictionary<string, List<CostumeData>>();

	private JitDictionary<int, CostumeData> _data_by_ProductIndex = new JitDictionary<int, CostumeData>();

	private JitRef<CostumeData> _data_of_NewAndNotFree = new JitRef<CostumeData>();

	public override TablePathDefine PathDefine => new TablePathDefine("CostumeTable");

	public CostumeDataContainer()
	{
		base.Packer.RegisterAndBind(_datas_by_HeroStar);
		base.Packer.RegisterAndBind(_datas_by_Group);
		base.Packer.RegisterAndBind(_data_by_ProductIndex);
		base.Packer.RegisterAndBind(_data_of_NewAndNotFree);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (CostumeData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_datas_by_HeroStar.JsonLoad_AddToListValue(new HeroAndStarKey(item.HeroIndex, item.HeroStar), item);
			if (!String_Extension_Method.IsNullOrEmpty(item.Group))
			{
				_datas_by_Group.JsonLoad_AddToListValue(item.Group, item);
			}
			if (!_data_by_ProductIndex.JsonLoad_Get().ContainsKey(item.ProductIndex))
			{
				_data_by_ProductIndex.JsonLoad_Get().Add(item.ProductIndex, item);
			}
			if (_data_of_NewAndNotFree.JsonLoad_IsNull() && !item.IsFreeCostume() && item.New != 0)
			{
				_data_of_NewAndNotFree.JsonLoad_Set(item);
			}
		}
	}

	public EHeroCostumeType GetHeroCostumeType(int costumeIndex)
	{
		return GetData(costumeIndex)?.CostumeType ?? EHeroCostumeType.None;
	}

	public CostumeData GetDataByHeroStar(int heroIndex, int heroStar)
	{
		List<CostumeData> orDefault = _datas_by_HeroStar.GetOrDefault(new HeroAndStarKey(heroIndex, heroStar));
		if (__Extension_Method_ICollection__.IsNullOrEmpty(orDefault))
		{
			return null;
		}
		return orDefault[0];
	}

	public List<CostumeData> GetCostumeDataListByHeroStar(int heroIndex, int heroStar, bool isFreeCostume)
	{
		List<CostumeData> list = new List<CostumeData>();
		List<CostumeData> orDefault = _datas_by_HeroStar.GetOrDefault(new HeroAndStarKey(heroIndex, heroStar));
		if (orDefault == null)
		{
			return list;
		}
		foreach (CostumeData item in orDefault)
		{
			if (item.IsDefault == isFreeCostume)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public bool NewNotFreeCostumeExists()
	{
		return _data_of_NewAndNotFree.Eval() != null;
	}

	public IList<CostumeData> GetDataByGroup(string group)
	{
		if (group == string.Empty)
		{
			return GetAllData();
		}
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_datas_by_Group.GetOrDefault(group));
	}

	public CostumeData GetDataByProductIndex(int productIndex)
	{
		return _data_by_ProductIndex.GetOrDefault(productIndex);
	}
}
