namespace NShared.GR2M_SyncWorldTier;

[Request]
public class Request
{
	public int TierIndex { get; set; }

	public GlobalAccountInfo[] AccountInfos { get; set; }
}
