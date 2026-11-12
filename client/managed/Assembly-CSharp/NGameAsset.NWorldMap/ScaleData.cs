using JsonBin;
using LitJson;

namespace NGameAsset.NWorldMap;

public class ScaleData : JsonBin.ILoadJson, IBuilder, LitJson.ILoadJson, IJsonInterface, IMakeJson
{
	public float PlayerMoveSpeed = 950f;

	public float PlayerScale = 140f;

	public float MapIndicatorScale = 140f;

	public float NodeScaleX = 1f;

	public float NodeScaleY = 1f;

	public float EdgeScale = 1f;

	bool JsonBin.ILoadJson.Load(JsonRead json)
	{
		PlayerMoveSpeed = json["PlayerMoveSpeed"].GETFLOAT(PlayerMoveSpeed);
		PlayerScale = json["PlayerScale"].GETFLOAT(PlayerScale);
		MapIndicatorScale = json["MapIndicatorScale"].GETFLOAT(MapIndicatorScale);
		NodeScaleX = json["NodeScaleX"].GETFLOAT(NodeScaleX);
		NodeScaleY = json["NodeScaleY"].GETFLOAT(NodeScaleY);
		EdgeScale = json["EdgeScale"].GETFLOAT(EdgeScale);
		return true;
	}

	bool LitJson.ILoadJson.Load(JsonData json)
	{
		PlayerMoveSpeed = json["PlayerMoveSpeed"].GETFLOAT(PlayerMoveSpeed);
		PlayerScale = json["PlayerScale"].GETFLOAT(PlayerScale);
		MapIndicatorScale = json["MapIndicatorScale"].GETFLOAT(MapIndicatorScale);
		NodeScaleX = json["NodeScaleX"].GETFLOAT(NodeScaleX);
		NodeScaleY = json["NodeScaleY"].GETFLOAT(NodeScaleY);
		EdgeScale = json["EdgeScale"].GETFLOAT(EdgeScale);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("PlayerMoveSpeed", PlayerMoveSpeed);
		jsonData.AddFieldDouble("PlayerScale", PlayerScale);
		jsonData.AddFieldDouble("MapIndicatorScale", MapIndicatorScale);
		jsonData.AddFieldDouble("NodeScaleX", NodeScaleX);
		jsonData.AddFieldDouble("NodeScaleY", NodeScaleY);
		jsonData.AddFieldDouble("EdgeScale", EdgeScale);
		return jsonData;
	}
}
