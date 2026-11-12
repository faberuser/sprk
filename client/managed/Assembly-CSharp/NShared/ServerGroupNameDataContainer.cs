using NShared.NJit;

namespace NShared;

public class ServerGroupNameDataContainer : JitDataContainerDefault<string, ServerGroupNameData>
{
	public const string TABLENAME = "ServerGroupNameTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ServerGroupNameTable");

	public string GetServerName(string serverGroup)
	{
		ServerGroupNameData data = GetData(serverGroup);
		if (data == null)
		{
			return serverGroup;
		}
		return data.ServerName;
	}
}
