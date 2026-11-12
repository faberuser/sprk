namespace NShared.M2GR_SyncWaiter;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public MatchInfo[] MatchCompletedInfos { get; set; }

	public long[] ExpiredAccountIds { get; set; }

	public string[] AllServerHosts { get; set; }
}
