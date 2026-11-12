using LitJson;

namespace NGame2.NGameSetting.NUi;

public class LoginSettingData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool ShowAgreement;

	public bool ShowLoginSelect;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		ShowAgreement = json["ShowAgreement"].GETBOOLEAN(i_default: false);
		ShowLoginSelect = json["ShowLoginSelect"].GETBOOLEAN(i_default: false);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("ShowAgreement", ShowAgreement);
		jsonData.AddFieldBool("ShowLoginSelect", ShowLoginSelect);
		return jsonData;
	}
}
