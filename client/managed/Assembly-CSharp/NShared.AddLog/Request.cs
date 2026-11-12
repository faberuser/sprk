namespace NShared.AddLog;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "client_log/add_log";

	public string TableName { get; set; }

	public long AccountId { get; set; }

	public int Newbie { get; set; }

	public int AssetQuality { get; set; }

	public int DownloadType { get; set; }

	public int Step { get; set; }

	public string BackgroundUID { get; set; }
}
