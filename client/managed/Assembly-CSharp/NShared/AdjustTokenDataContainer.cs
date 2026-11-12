using NShared.NJit;

namespace NShared;

public class AdjustTokenDataContainer : JitDataContainerDefault<int, AdjustTokenData>, IData_from_PreloadInAppBytes
{
	public const string TABLENAME = "DefaultTableData/AdjustTokenTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DefaultTableData/AdjustTokenTable");

	public bool ContainKey(string key)
	{
		foreach (AdjustTokenData allDatum in GetAllData())
		{
			if (allDatum.EventName.Equals(key))
			{
				return true;
			}
		}
		return false;
	}

	public string GetToken(string key)
	{
		foreach (AdjustTokenData allDatum in GetAllData())
		{
			if (allDatum.EventName.Equals(key))
			{
				return allDatum.Token;
			}
		}
		return string.Empty;
	}
}
