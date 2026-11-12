using NShared.NJit;

namespace NShared;

public class CreatureAwakeChallengeItemDataContainer : JitDataContainerDefault<CreatureTagType, CreatureAwakeChallengeItemData>
{
	public const string TABLENAME = "CreatureAwakeChallengeItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureAwakeChallengeItemTable");
}
