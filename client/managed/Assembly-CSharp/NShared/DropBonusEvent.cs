namespace NShared;

[JsonSerializable(Javascript = true)]
public class DropBonusEvent
{
	public int Id { get; set; }

	public string DayOfWeek { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public int BeginHour { get; set; }

	public int BeginMinute { get; set; }

	public int EndHour { get; set; }

	public int EndMinute { get; set; }

	public BattleType[] DungeonType { get; set; }

	public EventBonusType BonusType { get; set; }

	public string ItemCode { get; set; }

	public int[] ItemIndices { get; set; }

	public int BonusPercentage { get; set; }

	public string Condition { get; set; }

	public string Title { get; set; }

	public string[] Message { get; set; }

	public DropBonusEventLocalInfo[] LocalInfo { get; set; }

	public ClientActivityType MoveTaskType { get; set; }

	public string MoveTaskValue { get; set; }

	public int MinLevel { get; set; }

	public DropBonusEvnetBuffPosition BuffPosition { get; set; }
}
