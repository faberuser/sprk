namespace NShared;

[JsonSerializable(Javascript = true)]
public class BattleSimulationInfo
{
	public string RequestServer { get; set; }

	public string GroupId { get; set; }

	public string Uid { get; set; }

	public BattleSimulationAccountInfo[] AccountInfos { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int DungeonDifficulty { get; set; }

	public int RepeatCount { get; set; }

	public int RemainCount { get; set; }

	public BattleInstanceExtraData ExtraData { get; set; }
}
