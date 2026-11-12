using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KingsPassLevelData : IDataKeySelector<KingsPassLevelKey>
{
	[Key(0)]
	public int LevelIndex { get; set; }

	[Key(1)]
	public int Level { get; set; }

	[Key(2)]
	public int ReqExp { get; set; }

	[Key(3)]
	public int LevelUpPrice { get; set; }

	KingsPassLevelKey IDataKeySelector<KingsPassLevelKey>.Key => new KingsPassLevelKey(LevelIndex, Level);

	public override string ToString()
	{
		return "LevelIndex=" + LevelIndex + "Level =" + Level;
	}
}
