using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildLevelData : IDataKeySelector<int>
{
	[Key(0)]
	public int Level { get; set; }

	[Key(1)]
	public int ActivityPoint { get; set; }

	[Key(2)]
	public int TotalActivityPoint { get; set; }

	[Key(3)]
	public int MaxMember { get; set; }

	[Key(4)]
	public int MaxAdmin { get; set; }

	int IDataKeySelector<int>.Key => Level;

	public override string ToString()
	{
		return "Level=" + Level;
	}
}
