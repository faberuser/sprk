using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipItemLevelData : IDataKeySelector<int>
{
	[Key(0)]
	public int Level { get; set; }

	[Key(1)]
	public int OptionOpen { get; set; }

	int IDataKeySelector<int>.Key => Level;

	public override string ToString()
	{
		return "Level=" + Level;
	}
}
