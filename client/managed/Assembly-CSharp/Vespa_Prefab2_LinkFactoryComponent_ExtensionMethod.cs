using NVespa.NPrefab2;

public static class Vespa_Prefab2_LinkFactoryComponent_ExtensionMethod
{
	public static T GetLinkData<T>(this LinkFactoryComponent This) where T : class, ILinkData
	{
		if (This == null)
		{
			return null;
		}
		return This._linkData as T;
	}
}
