using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AchievementTextData : IDataKeySelector<AchievementKey2>
{
	AchievementKey2 IDataKeySelector<AchievementKey2>.Key => new AchievementKey2(Index, Step);

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Step { get; set; }

	[Key(2)]
	public string AchievementNameTextKey { get; set; }

	[Key(3)]
	public string AchievementDescTextKey { get; set; }

	[Key(4)]
	public int DisplayValue { get; set; }

	public override string ToString()
	{
		return "AchievementTextData Index=" + Index;
	}
}
