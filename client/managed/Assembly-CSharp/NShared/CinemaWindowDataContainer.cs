using NShared.NJit;

namespace NShared;

public class CinemaWindowDataContainer : JitDataContainerDefault<string, CinemaWindowData>
{
	public const string TABLENAME = "CinemaWindowTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CinemaWindowTable");
}
