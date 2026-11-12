using System.Linq;
using NShared.NJit;

namespace NShared;

public class PlayerArchiveDataContainer : JitDataContainerDefault<PlayerArchiveDataKey, PlayerArchiveData>
{
	public const string TABLENAME = "PlayerArchiveTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PlayerArchiveTable");

	public PlayerArchiveData GetData(int mainIndex, int subIndex)
	{
		return GetData(new PlayerArchiveDataKey(mainIndex, subIndex));
	}

	public PlayerArchiveData[] GetDataByMainType(ArchiveMainType mainType)
	{
		return (from v in GetAllData()
			where v.ArchiveMainType == mainType
			select v).ToArray();
	}

	public PlayerArchiveData[] GetDataBySubType(ArchiveSubType subType, ArchiveSubType2 subType2 = ArchiveSubType2.None)
	{
		return GetAllData().Where(delegate(PlayerArchiveData v)
		{
			if (v.ArchiveSubType != subType)
			{
				return false;
			}
			return (subType2 == ArchiveSubType2.None || v.ArchiveSubType2 == subType2) ? true : false;
		}).ToArray();
	}
}
