using NShared.NUtil;

namespace NShared;

public class PlayRecordCondition
{
	public string Key { get; set; }

	public CheckType Compare { get; set; }

	public string[] Value { get; set; }
}
