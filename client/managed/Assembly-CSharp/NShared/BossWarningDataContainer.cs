using System.Collections;
using NShared.NJit;

namespace NShared;

public class BossWarningDataContainer : JitDataContainerDefault<int, BossWarningData>
{
	public const string TABLENAME = "BossWarningTable";

	public override TablePathDefine PathDefine => new TablePathDefine("BossWarningTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, BossWarningData value)
	{
		if (value.DurationTime == 0f)
		{
			value.DurationTime = 1000f;
		}
	}
}
