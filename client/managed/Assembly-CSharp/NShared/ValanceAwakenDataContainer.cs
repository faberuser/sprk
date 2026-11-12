using NShared.NJit;

namespace NShared;

public class ValanceAwakenDataContainer : JitDataContainerDefault<ValanceAwakenDataKey, ValanceAwakenData>
{
	public const string TABLENAME = "ValanceAwakenTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ValanceAwakenTable");

	public ValanceAwakenData GetData(byte tier, int detailIndex, CreatureTagType creatureTagType, int star)
	{
		return GetData(new ValanceAwakenDataKey(tier, detailIndex, creatureTagType, star));
	}
}
