using NGame2.NUI;

public static class Game2_UI_UIBundleLoader_ExtensionMethod
{
	public static UITexture UITEX(this UIBundleLoader This)
	{
		if (!(This == null))
		{
			return This.UiTex;
		}
		return null;
	}
}
