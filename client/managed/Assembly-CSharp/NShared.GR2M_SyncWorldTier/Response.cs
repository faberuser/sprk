namespace NShared.GR2M_SyncWorldTier;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public string ServerGroup { get; set; }

	public string[] AffectedAccountIds { get; set; }
}
