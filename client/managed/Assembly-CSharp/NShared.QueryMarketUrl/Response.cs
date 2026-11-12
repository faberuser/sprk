namespace NShared.QueryMarketUrl;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PopupTypes PopupType { get; set; }

	public string Version { get; set; }

	public string MarketUrl { get; set; }

	public string ReservedStr { get; set; }

	public string PtsMarketUrl { get; set; }
}
