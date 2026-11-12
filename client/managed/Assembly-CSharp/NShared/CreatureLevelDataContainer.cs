using System.Linq;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class CreatureLevelDataContainer : JitDataContainerDefault<int, CreatureLevelData>, IExpGetter
{
	[MessagePackObject(false)]
	public class ContainerInfo
	{
		[Key(0)]
		public int MinLevel { get; set; }

		[Key(1)]
		public int MaxLevel { get; set; }

		[Key(2)]
		public long LastTotalExp { get; set; }
	}

	public const string TABLENAME = "CreatureLevelTable";

	private JitRef<ContainerInfo> _containerInfo = new JitRef<ContainerInfo>();

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureLevelTable");

	public int MaxLevel => _containerInfo.Eval().MaxLevel;

	public CreatureLevelDataContainer()
	{
		base.Packer.RegisterAndBind(_containerInfo);
	}

	protected override void OnEndLoadFromJsonData()
	{
		_containerInfo.JsonLoad_GetOrNew().MinLevel = (from v in base.BaseList.JsonLoad_GetOrNew()
			select v.Level).DefaultIfEmpty(0).Min();
		_containerInfo.JsonLoad_GetOrNew().MaxLevel = (from v in base.BaseList.JsonLoad_GetOrNew()
			select v.Level).DefaultIfEmpty(0).Max();
		foreach (CreatureLevelData item in base.BaseList.JsonLoad_GetOrNew())
		{
			item.TotalExp = _containerInfo.JsonLoad_GetOrNew().LastTotalExp;
			_containerInfo.JsonLoad_GetOrNew().LastTotalExp += item.LocalExp;
		}
	}

	public long GetTotalExp(int level, long localExp)
	{
		if (level >= _containerInfo.Eval().MaxLevel)
		{
			return _containerInfo.Eval().LastTotalExp;
		}
		return (GetData(level)?.TotalExp ?? 0) + localExp;
	}

	public CreatureLevelData GetDataByExp(long totalExp, bool isNextLevelData = false)
	{
		CreatureLevelData result = null;
		foreach (CreatureLevelData allDatum in GetAllData())
		{
			if (allDatum.TotalExp > totalExp)
			{
				if (isNextLevelData)
				{
					result = allDatum;
				}
				break;
			}
			result = allDatum;
		}
		return result;
	}

	public void ParseTotalExpToLocal(long totalExp, out int level, out long localExp, out long localExpOfLevel)
	{
		level = 1;
		localExp = 0L;
		localExpOfLevel = 0L;
		CreatureLevelData dataByExp = GetDataByExp(totalExp);
		if (dataByExp != null)
		{
			level = dataByExp.Level;
			localExp = totalExp - dataByExp.TotalExp;
			localExpOfLevel = dataByExp.LocalExp;
		}
	}

	long IExpGetter.GetLocalExp(int level)
	{
		return GetData(level)?.LocalExp ?? 0;
	}
}
