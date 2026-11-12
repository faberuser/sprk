using NGame2.NRender;

public static class ExtensionMethod_Game2_Render_RenderFacade
{
	public static RenderFacade DISABLEGROUNDCLIP(this RenderFacade This)
	{
		if (This == null)
		{
			return null;
		}
		This.AssetCreature.GetRenderGroupModifier().SetGroundClip(enable: false);
		return This;
	}
}
