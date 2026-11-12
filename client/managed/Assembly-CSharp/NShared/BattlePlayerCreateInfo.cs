namespace NShared;

[JsonSerializable]
public class BattlePlayerCreateInfo
{
	public long Id { get; set; }

	public TeamType TeamType { get; set; }
}
