namespace NShared;

[JsonSerializable(Javascript = true)]
public class ClassBuffPointInfo
{
	public int TagType { get; set; }

	public long BuffPoint { get; set; }
}
