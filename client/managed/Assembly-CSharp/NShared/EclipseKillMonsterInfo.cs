namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseKillMonsterInfo
{
	public int CreatureIndex { get; set; }

	public int KillCount { get; set; }
}
