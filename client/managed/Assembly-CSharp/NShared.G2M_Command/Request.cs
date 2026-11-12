namespace NShared.G2M_Command;

[Request]
public class Request
{
	public string Host { get; set; }

	public string Namespace { get; set; }

	public string Path { get; set; }

	public string Service { get; set; }

	public string Method { get; set; }

	public string[] Argv { get; set; }
}
