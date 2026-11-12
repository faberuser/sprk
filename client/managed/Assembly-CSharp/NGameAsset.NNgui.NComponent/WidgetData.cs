using JsonBin;

namespace NGameAsset.NNgui.NComponent;

public class WidgetData : ILoadJson
{
	public enum EAppearanceType
	{
		Background,
		Popup,
		Last
	}

	public string Path;

	public EAppearanceType AppearanceType = EAppearanceType.Popup;

	bool ILoadJson.Load(JsonRead json)
	{
		Path = json["Path"].GETSTRING(Path);
		AppearanceType = json["AppearanceType"].GETENUM(AppearanceType);
		return true;
	}
}
