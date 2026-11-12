using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class TeamLevelCommonOptionOrderData : IDataKeySelector<OperationType>
{
	[Key(0)]
	public OperationType OptionType { get; set; }

	[Key(1)]
	public int Priority { get; set; }

	OperationType IDataKeySelector<OperationType>.Key => OptionType;
}
