using NVespa.NGlobal;

namespace NShared;

public static class LocalizeProxyBuilder
{
	public static ILocalizeProxy CreateDefault()
	{
		return new LocalizeProxy_Client();
	}
}
