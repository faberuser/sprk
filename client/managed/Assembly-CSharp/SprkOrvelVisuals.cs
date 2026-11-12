using NShared;

public static class SprkOrvelVisuals
{
	public static WorldMapPropData FilterProp(WorldMapPropData prop)
	{
		if (prop == null || prop.ChapterIndex != 99 || (!(prop.PropName == "UI.EventPopup") && !(prop.PropName == "UI.EventPopup3") && !(prop.PropName == "UI.EventPopupFire") && !(prop.PropName == "UI.Node.Portal") && !(prop.PropName == "UI.Node.HeroDungeon")))
		{
			return prop;
		}
		return null;
	}
}
