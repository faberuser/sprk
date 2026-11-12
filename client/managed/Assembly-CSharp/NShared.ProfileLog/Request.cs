namespace NShared.ProfileLog;

[Request]
[JsonSerializable(Javascript = true)]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "log/add_logs";

	public string[] LogItems { get; set; }
}
