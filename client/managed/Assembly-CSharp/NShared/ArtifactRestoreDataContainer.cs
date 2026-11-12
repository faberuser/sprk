using NShared.NJit;

namespace NShared;

public class ArtifactRestoreDataContainer : JitDataContainerDefault<int, ArtifactRestoreData>
{
	public const string TABLENAME = "ArtifactRestoreTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ArtifactRestoreTable");
}
