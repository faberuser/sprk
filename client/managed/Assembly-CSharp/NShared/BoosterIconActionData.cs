using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class BoosterIconActionData : IDataKeySelector<string>
{
	[Key(0)]
	public string EventBonusType { get; set; }

	[Key(1)]
	public string MoveTaskType { get; set; }

	[Key(2)]
	public string[] MoveTaskValue { get; set; }

	string IDataKeySelector<string>.Key => EventBonusType;
}
