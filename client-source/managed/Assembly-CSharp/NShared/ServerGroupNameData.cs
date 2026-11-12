using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ServerGroupNameData : IDataKeySelector<string>
{
	[Key(0)]
	public string ServerGroup { get; set; }

	[Key(1)]
	public string ServerName { get; set; }

	string IDataKeySelector<string>.Key => ServerGroup;
}
