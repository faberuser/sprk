using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureGradeData : IDataKeySelector<int>
{
	[Key(0)]
	public int Grade { get; set; }

	[Key(1)]
	public int Level { get; set; }

	[Key(2)]
	public int Gold { get; set; }

	[Key(3)]
	public int Stone { get; set; }

	int IDataKeySelector<int>.Key => Grade;

	public override string ToString()
	{
		return "Grade=" + Grade;
	}
}
