using NShared.NJit;

namespace NShared;

public class EntryMenuSettingDataContainer : JitDataContainerDefault<string, EntryMenuSettingData>
{
	public const string TABLENAME = "EntryMenuSettingTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EntryMenuSettingTable");

	public EntryMenuSettingData GetDataByMenuType(string key)
	{
		return GetData(key);
	}
}
