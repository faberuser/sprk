using System.Collections.Generic;

namespace NShared;

public class ProbabilityFrameData<T> where T : ProbabilityBaseData
{
	public int MainIndex;

	public string MainName;

	public List<T> ProbabilityInfoList;

	public ProbabilityFrameData(int mainIndex, string mainName, List<T> probabilityinfoList)
	{
		MainIndex = mainIndex;
		MainName = mainName;
		ProbabilityInfoList = probabilityinfoList;
	}
}
