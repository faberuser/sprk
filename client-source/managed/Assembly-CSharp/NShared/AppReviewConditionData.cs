using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AppReviewConditionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string[] ModifyCondition { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "AppReviewConditionData Index=" + Index;
	}
}
