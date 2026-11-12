namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerArchiveInfo : IAccountAttrKey<PlayerArchiveKey>
{
	PlayerArchiveKey IAccountAttrKey<PlayerArchiveKey>.Key => new PlayerArchiveKey(ArchiveMainIndex, ArchiveSubIndex);

	public int ArchiveMainIndex { get; set; }

	public int ArchiveSubIndex { get; set; }

	public string ArchiveValue { get; set; }

	public string UpdatedTime { get; set; }
}
