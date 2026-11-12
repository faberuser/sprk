using NVespa;

public static class Vespa_SmallPooling_ExtensionMethod
{
	public static T CaptureSmallInstance<T>(this T This) where T : SmallPooling<T>, ISmallPoolingItem, new()
	{
		return SmallPooling<T>.CaptureInstance(This);
	}
}
