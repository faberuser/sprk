using System;
using JsonBin;
using LitJson;
using UnityEngine;

namespace NVespa;

[Serializable]
public class TransformData : ITransformData, JsonBin.ILoadJson, IBuilder, LitJson.ILoadJson, IJsonInterface, IMakeJson
{
	public Vector3 Position = Vector3.zero;

	public Vector3 RotationEuler = Vector3.zero;

	public float Scale = 1f;

	Vector3 ITransformData.Position => Position;

	Quaternion ITransformData.Rotation => Rotation;

	float ITransformData.Scale => Scale;

	public Quaternion Rotation => Quaternion.Euler(RotationEuler);

	public TransformData()
	{
	}

	public TransformData(float scale)
	{
		Scale = scale;
	}

	public TransformData Clone()
	{
		return new TransformData
		{
			Position = Position,
			RotationEuler = RotationEuler,
			Scale = Scale
		};
	}

	public ITransformData Convert(Transform from, Transform to)
	{
		Vector3 position = to.worldToLocalMatrix.MultiplyPoint(from.localToWorldMatrix.MultiplyPoint(Position));
		Quaternion rotation = Quaternion.Inverse(to.rotation) * (from.rotation * Rotation);
		float scale = Scale * Vespa_Unity_Vector2_Extension_Method.MaxComponent(Vespa_Unity_Vector3_Extension_Method.XY(from.lossyScale)) / Vespa_Unity_Vector2_Extension_Method.MaxComponent(Vespa_Unity_Vector3_Extension_Method.XY(to.lossyScale));
		return new TransformData2
		{
			Position = position,
			Rotation = rotation,
			Scale = scale
		};
	}

	bool JsonBin.ILoadJson.Load(JsonRead json)
	{
		Position = json["Position"].GETVECTOR3(Position);
		RotationEuler = json["RotationEuler"].GETVECTOR3(RotationEuler);
		Scale = json["Scale"].GETFLOAT(Scale);
		return true;
	}

	bool LitJson.ILoadJson.Load(JsonData json)
	{
		Position = json["Position"].GETVECTOR3(Position);
		RotationEuler = json["RotationEuler"].GETVECTOR3(RotationEuler);
		Scale = json["Scale"].GETFLOAT(Scale);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldVector3("Position", Position);
		jsonData.AddFieldVector3("RotationEuler", RotationEuler);
		jsonData.AddFieldDouble("Scale", Scale);
		return jsonData;
	}
}
