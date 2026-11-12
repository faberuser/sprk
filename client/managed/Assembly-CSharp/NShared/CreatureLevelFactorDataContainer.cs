using System.Linq;
using NShared.NJit;

namespace NShared;

public class CreatureLevelFactorDataContainer : JitDataContainerDefault<CreatureLevelFactorDataKey, CreatureLevelFactorData>
{
	public const string TABLENAME = "CreatureLevelFactorTable";

	private JitDictionary<int, short> _maxLevels_by_FactorIndex = new JitDictionary<int, short>();

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureLevelFactorTable");

	public CreatureLevelFactorDataContainer()
	{
		base.Packer.RegisterAndBind(_maxLevels_by_FactorIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (int factorIndex in (from v in base.BaseList.JsonLoad_GetOrNew()
			select v.FactorIndex).Distinct().ToList())
		{
			_maxLevels_by_FactorIndex.JsonLoad_Get()[factorIndex] = (from v in base.BaseList.JsonLoad_GetOrNew()
				where v.FactorIndex == factorIndex
				select v.Level).Max();
		}
	}

	public short GetMaxLevel(CreatureLevelFactorData.FactorType factorType)
	{
		return _maxLevels_by_FactorIndex.GetOrDefault((int)factorType);
	}
}
