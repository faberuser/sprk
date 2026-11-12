using NShared.NJit;

namespace NShared;

public class GameTitleSceneIntroduceDataContainer : JitDataContainerDefault<int, GameTitleSceneIntroduceData>, IData_from_PreloadInAppBytes
{
	public const string TABLENAME = "DefaultTableData/GameTitleSceneIntroduceTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DefaultTableData/GameTitleSceneIntroduceTable");
}
