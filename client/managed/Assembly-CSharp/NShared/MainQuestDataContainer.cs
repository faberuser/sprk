using NShared.NJit;

namespace NShared;

public class MainQuestDataContainer : JitDataContainerDefault<int, MainQuestData>
{
	public const string TABLENAME = "MainQuestTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MainQuestTable");

	public MainQuestData GetDataByStep(int step)
	{
		foreach (MainQuestData allDatum in GetAllData())
		{
			if (allDatum.Step == step)
			{
				return allDatum;
			}
		}
		return null;
	}

	public bool IsClear(int step, int progress)
	{
		MainQuestData dataByStep = GetDataByStep(step);
		if (dataByStep == null)
		{
			return false;
		}
		if (dataByStep.GetReqProgress() > progress)
		{
			return false;
		}
		return true;
	}

	public bool IsClearLastStep(int step)
	{
		int num = 0;
		foreach (MainQuestData allDatum in GetAllData())
		{
			if (num < allDatum.Step)
			{
				num = allDatum.Step;
			}
		}
		if (step <= num)
		{
			return false;
		}
		return true;
	}
}
