using JsonBin;

namespace NGameAsset.NNgui;

public class NguiData
{
	public string Code = string.Empty;

	public string BundleFileName = string.Empty;

	public NguiComponentData NguiComponent = new NguiComponentData();

	public bool Open(string code, JsonRead json)
	{
		Code = code;
		if (!json.IsObject())
		{
			return false;
		}
		if (!LoadFromJson(json))
		{
			return false;
		}
		return true;
	}

	private bool LoadFromJson(JsonRead json)
	{
		string sTRING = json["BundleTypeName"].GETSTRING("(empty)");
		string sTRING2 = json["VersionCode"].GETSTRING("(empty)");
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING, "[Ngui]"))
		{
			return false;
		}
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING2, "20150812"))
		{
			return false;
		}
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		NguiComponent.LOADFROMJSON(json["WidgetLink"]);
		return true;
	}
}
