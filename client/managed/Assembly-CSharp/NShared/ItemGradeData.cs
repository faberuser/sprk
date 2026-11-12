using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class ItemGradeData : IDataKeySelector<int>
{
	[Key(0)]
	public int Grade { get; set; }

	[Key(1)]
	public byte Score { get; set; }

	[Key(2)]
	public string GetEffect { get; set; }

	[Key(3)]
	public string NameColor { get; set; }

	int IDataKeySelector<int>.Key => Grade;

	public override string ToString()
	{
		return "Grade=" + Grade;
	}
}
