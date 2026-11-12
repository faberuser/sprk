using NShared.NJit;

namespace NShared;

public class AppReviewConditionDataContainer : JitDataContainerDefault<int, AppReviewConditionData>
{
	public const string TABLENAME = "AppReviewConditionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AppReviewConditionTable");

	public bool IsCheck(string[] values)
	{
		foreach (AppReviewConditionData allDatum in GetAllData())
		{
			string[] modifyCondition = allDatum.ModifyCondition;
			if (values.Length != modifyCondition.Length)
			{
				continue;
			}
			bool flag = true;
			int num = 0;
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i] != modifyCondition[num])
				{
					flag = false;
					break;
				}
				num++;
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}
}
