using NGame2.NUI;
using NGame2.NUI.NWindow;

public static class Game2_UI_Window_LobbyNodeFunctionMenu_ExtensionMethod
{
	public static void ShowButton(this LobbyNodeFunctionMenu.DungeonButton This, bool enabled)
	{
		if (This != null)
		{
			UIHelper.EnableGameObject(This.ButtonWidget, enabled);
		}
	}

	public static void EnableButton(this LobbyNodeFunctionMenu.DungeonButton This, bool enabled)
	{
		if (This != null)
		{
			UIHelper.SetWidgetGrayScale(This.ButtonWidget, !enabled);
		}
	}
}
