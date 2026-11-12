namespace NShared;

public class PlayerArchiveKey : Tuple<int, int>
{
	public PlayerArchiveKey(int archiveMainIndex, int archiveSubIndex)
		: base(archiveMainIndex, archiveSubIndex)
	{
	}
}
