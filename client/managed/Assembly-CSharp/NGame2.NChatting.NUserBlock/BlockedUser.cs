using LitJson;

namespace NGame2.NChatting.NUserBlock;

public class BlockedUser
{
	public long AccountId;

	public string UserName;

	public static BlockedUser CreateFromJson(JsonData jsonData)
	{
		JsonData jsonData2 = jsonData["AccountId"];
		if (jsonData2 == null)
		{
			return null;
		}
		JsonData jsonData3 = jsonData["UserName"];
		if (jsonData3 == null)
		{
			return null;
		}
		long lONG = jsonData2.GETLONG(0L);
		string sTRING = jsonData3.GETSTRING(string.Empty);
		return new BlockedUser
		{
			AccountId = lONG,
			UserName = sTRING
		};
	}

	public string ToJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("AccountId", AccountId);
		jsonData.AddFieldString("UserName", UserName);
		return jsonData.ToJson();
	}

	public override string ToString()
	{
		return ToJson();
	}
}
