using NShared.NJit;

namespace NShared;

public class CraftInstantGemDataContainer : JitDataContainerListOnly<CraftInstantGemData>
{
	public const string TABLENAME = "CraftInstantGemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CraftInstantGemTable");

	public CraftInstantGemData GetData(int remainTime)
	{
		CraftInstantGemData result = new CraftInstantGemData();
		foreach (CraftInstantGemData allDatum in GetAllData())
		{
			if (allDatum.MinRemainTime <= remainTime && allDatum.MaxRemainTime >= remainTime)
			{
				result = allDatum;
				break;
			}
		}
		return result;
	}

	public int GetReqGem(int remainTime)
	{
		CraftInstantGemData data = GetData(remainTime);
		if (data.MinRemainTime == 0 && data.MaxRemainTime == 0)
		{
			return 0;
		}
		return data.GetReqGem(remainTime);
	}
}
