namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class RightAwayProductEvent
{
	public RightAwayProductInfo[] ProductInfos { get; set; }

	public int[] RemovedIndices { get; set; }
}
