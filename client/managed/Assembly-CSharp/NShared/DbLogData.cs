using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class DbLogData : IDataKeySelector<DbLogDataKey>
{
	[Key(0)]
	public string Category { get; set; }

	[Key(1)]
	public int Value { get; set; }

	[Key(2)]
	public string Name { get; set; }

	[Key(3)]
	public string Type { get; set; }

	DbLogDataKey IDataKeySelector<DbLogDataKey>.Key => new DbLogDataKey(Category, Value);
}
