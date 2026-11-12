namespace NShared;

[JsonSerializable]
public class ApiErrorLog
{
	public long AccountId { get; set; }

	public string LoginId { get; set; }

	public string Version { get; set; }

	public long Seq { get; set; }

	public string PreApi { get; set; }

	public string Api { get; set; }

	public string HttpError { get; set; }

	public string ServerError { get; set; }

	public string Message { get; set; }

	public string ServerMessage { get; set; }
}
