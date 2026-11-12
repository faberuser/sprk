namespace NVespa.NWeb;

public interface IServiceDisabler
{
	bool IsDisabled(string host, string serviceName);
}
