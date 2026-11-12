using JsonBin;
using LitJson;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class FieldCameraData : JsonBin.ILoadJson, IBuilder, LitJson.ILoadJson, IJsonInterface, IMakeJson
{
	public float LookAngle = 25f;

	public float HorizonAngle;

	public float FieldOfView = 25f;

	public float MinHeight = 7f;

	public bool FitToField = true;

	public Vector3 BoundsMargin = Vector3.one;

	public Vector3 BoundsOffset = Vector3.zero;

	public string NextCameraName = "";

	bool JsonBin.ILoadJson.Load(JsonRead json)
	{
		LookAngle = json["LookAngle"].GETFLOAT(LookAngle);
		HorizonAngle = json["HorizonAngle"].GETFLOAT(HorizonAngle);
		FieldOfView = json["FieldOfView"].GETFLOAT(FieldOfView);
		MinHeight = json["MinHeight"].GETFLOAT(MinHeight);
		FitToField = json["FitToField"].GETBOOLEAN(FitToField);
		BoundsMargin = json["BoundsMargin"].GETVECTOR3(BoundsMargin);
		BoundsOffset = json["BoundsOffset"].GETVECTOR3(BoundsOffset);
		NextCameraName = json["NextCameraName"].GETSTRING(NextCameraName);
		return true;
	}

	bool LitJson.ILoadJson.Load(JsonData json)
	{
		LookAngle = json["LookAngle"].GETFLOAT(LookAngle);
		HorizonAngle = json["HorizonAngle"].GETFLOAT(HorizonAngle);
		FieldOfView = json["FieldOfView"].GETFLOAT(FieldOfView);
		MinHeight = json["MinHeight"].GETFLOAT(MinHeight);
		FitToField = json["FitToField"].GETBOOLEAN(FitToField);
		BoundsMargin = json["BoundsMargin"].GETVECTOR3(BoundsMargin);
		BoundsOffset = json["BoundsOffset"].GETVECTOR3(BoundsOffset);
		NextCameraName = json["NextCameraName"].GETSTRING(NextCameraName);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("LookAngle", LookAngle);
		jsonData.AddFieldDouble("HorizonAngle", HorizonAngle);
		jsonData.AddFieldDouble("FieldOfView", FieldOfView);
		jsonData.AddFieldDouble("MinHeight", MinHeight);
		jsonData.AddFieldBool("FitToField", FitToField);
		jsonData.AddFieldVector3("BoundsMargin", BoundsMargin);
		jsonData.AddFieldVector3("BoundsOffset", BoundsOffset);
		jsonData.AddFieldString("NextCameraName", NextCameraName);
		return jsonData;
	}

	public FieldCameraData Clone()
	{
		return new FieldCameraData
		{
			BoundsMargin = BoundsMargin,
			BoundsOffset = BoundsOffset,
			FieldOfView = FieldOfView,
			FitToField = FitToField,
			HorizonAngle = HorizonAngle,
			LookAngle = LookAngle,
			MinHeight = MinHeight,
			NextCameraName = NextCameraName
		};
	}
}
