using NGame2.NRender;

public static class ExtensionMethod_Game2_Render_GuildFlagFacade
{
	public static void ReleaseSelf(this GuildFlagFacade This)
	{
		if (!(This == null))
		{
			This.Close();
			VespaObject.PendingDestroyGo(This);
		}
	}
}
