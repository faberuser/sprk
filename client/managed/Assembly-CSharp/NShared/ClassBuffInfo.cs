namespace NShared;

[JsonSerializable(Javascript = true)]
public class ClassBuffInfo : IAccountAttrKey<ClassBuffKey>
{
	ClassBuffKey IAccountAttrKey<ClassBuffKey>.Key => new ClassBuffKey(TagType, ClassBuffIndex);

	public int TagType { get; set; }

	public int ClassBuffIndex { get; set; }

	public int ClassBuffLevel { get; set; }

	public string UpdatedTime { get; set; }
}
