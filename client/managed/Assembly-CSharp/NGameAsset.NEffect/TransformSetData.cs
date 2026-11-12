using JsonBin;
using LitJson;
using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect;

public class TransformSetData : JsonBin.ILoadJson, IBuilder, LitJson.ILoadJson, IJsonInterface, IMakeJson
{
	public Vector3 Position = Vector3.zero;

	public Vector3 RotationEuler = Vector3.zero;

	public Vector3 Scale = Vector3.one;

	public Quaternion Rotation => Quaternion.Euler(RotationEuler);

	public void AssignTransform(Transform tm, ITransformData extraTransform)
	{
		if (extraTransform != null)
		{
			tm.localPosition = extraTransform.Position + extraTransform.Rotation * (Position * extraTransform.Scale);
			tm.localRotation = extraTransform.Rotation * Rotation;
			tm.localScale = extraTransform.Scale * Scale;
		}
		else
		{
			tm.localPosition = Position;
			tm.localRotation = Rotation;
			tm.localScale = Scale;
		}
	}

	bool JsonBin.ILoadJson.Load(JsonRead json)
	{
		Position = json["Position"].GETVECTOR3(Position);
		RotationEuler = json["RotationEuler"].GETVECTOR3(RotationEuler);
		Scale = json["Scale"].GETVECTOR3(Scale);
		return true;
	}

	bool LitJson.ILoadJson.Load(JsonData json)
	{
		Position = JsonTools.ToVector3(json["Position"], Position);
		RotationEuler = JsonTools.ToVector3(json["RotationEuler"], RotationEuler);
		Scale = JsonTools.ToVector3(json["Scale"], Scale);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("Position", JsonTools.FromVector3(Position));
		jsonData.AddField("RotationEuler", JsonTools.FromVector3(RotationEuler));
		jsonData.AddField("Scale", JsonTools.FromVector3(Scale));
		return jsonData;
	}
}
