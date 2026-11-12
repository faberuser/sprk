using JsonBin;

namespace NGameAsset.NSound;

public class BgmData : ILoadJson
{
	public string Code = string.Empty;

	public string Name;

	public int Volume;

	bool ILoadJson.Load(JsonRead json)
	{
		Code = json["Code"].GETSTRING(string.Empty);
		Name = json["Name"].GETSTRING(string.Empty);
		if (String_Extension_Method.IsNullOrEmpty(Code) || String_Extension_Method.IsNullOrEmpty(Name))
		{
			return false;
		}
		Volume = json["Volume"].GETINT(1000);
		return true;
	}
}
