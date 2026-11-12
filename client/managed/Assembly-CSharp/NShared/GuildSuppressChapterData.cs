using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildSuppressChapterData : IDataKeySelector<int>
{
	[Key(0)]
	public int RaidIndex { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public string Description { get; set; }

	[Key(3)]
	public string BossInfoImage { get; set; }

	int IDataKeySelector<int>.Key => RaidIndex;

	public override string ToString()
	{
		return "GuildSuppressChapterIndex=" + RaidIndex;
	}
}
