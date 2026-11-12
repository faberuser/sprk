namespace NShared;

[JsonSerializable(Javascript = true)]
public class DOWDungeonInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => DungeonIndex;

	public int DungeonIndex { get; set; }

	public int Diff { get; set; }

	public string StartTime { get; set; }

	public string EndTime { get; set; }
}
