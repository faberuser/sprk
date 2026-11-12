using NShared.NJit;

namespace NShared;

public class ClientCustomizingCameraDataContainer : JitDataContainerDefault<AccessoryPartType, ClientCustomizingCameraData>
{
	public const string TABLENAME = "ClientCustomizingCameraTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ClientCustomizingCameraTable");
}
