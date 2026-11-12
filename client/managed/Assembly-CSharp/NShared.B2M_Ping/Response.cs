namespace NShared.B2M_Ping;

[BattlePacket]
public class Response
{
	public ResultType Result { get; set; }

	public string ErrorMessage { get; set; }

	public BattleIntegrityCheckInfo[] BattleIntegrityCheckInfos { get; set; }

	public ContentsValueInfo[] ContentsValues { get; set; }

	public ContentsValueInfo[] ServerValues { get; set; }

	public ConstantData[] ServerConstantValues { get; set; }
}
