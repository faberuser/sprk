using LitJson;
using UnityEngine;

namespace NGame2.NNodeTree.NUtil.NMaterial;

public class TextureTransformData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public enum EMethodType
	{
		Static,
		TileAnimationHorizon,
		TileAnimationVertical
	}

	public EMethodType MethodType;

	public Vector2 StaticOffset;

	public Vector2 StaticScale = Vector2.one;

	public AnimationCurve FrameCurve = new AnimationCurve();

	public int TileXCount = 1;

	public int TileYCount = 1;

	public int TileXUsing;

	public int TileYUsing;

	public int TileLoopCount = 1;

	public void InitMaterial(Material material, float initFrame)
	{
		if (MethodType == EMethodType.Static)
		{
			material.mainTextureOffset = StaticOffset;
			material.mainTextureScale = StaticScale;
		}
		else
		{
			material.mainTextureScale = new Vector2(1f / (float)TileXCount, 1f / (float)TileYCount);
			UpdateMaterial(material, initFrame);
		}
	}

	public void UpdateMaterial(Material material, float frame)
	{
		if (MethodType != EMethodType.Static && (MethodType == EMethodType.TileAnimationHorizon || MethodType == EMethodType.TileAnimationVertical))
		{
			CalculateTileOffset(frame, out var texOffset);
			material.mainTextureOffset = texOffset;
		}
	}

	public void Calculate(float frame, out Vector2 texOffset, out Vector2 texScale)
	{
		if (MethodType == EMethodType.Static)
		{
			texOffset = StaticOffset;
			texScale = StaticScale;
		}
		else if (MethodType == EMethodType.TileAnimationHorizon || MethodType == EMethodType.TileAnimationVertical)
		{
			CalculateTileOffset(frame, out texOffset);
			texScale = new Vector2(1f / (float)TileXCount, 1f / (float)TileYCount);
		}
		else
		{
			texOffset = StaticOffset;
			texScale = StaticScale;
		}
	}

	public void CalculateTileOffset(float frame, out Vector2 texOffset)
	{
		int num = ((TileXUsing < 1) ? TileXCount : TileXUsing);
		int num2 = ((TileYUsing < 1) ? TileYCount : TileYUsing);
		float num3 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(FrameCurve, frame, frame);
		int num4 = num * num2 * TileLoopCount;
		int num5 = Mathf.Min(num4 - 1, (int)((float)num4 * num3)) % (num * num2);
		int num6;
		int num7;
		if (MethodType == EMethodType.TileAnimationHorizon)
		{
			num6 = num5 % num;
			num7 = num5 / num;
		}
		else
		{
			num6 = num5 / num2;
			num7 = num5 % num2;
		}
		num7 = TileYCount - num7 - 1;
		texOffset = new Vector2((float)num6 / (float)TileXCount, (float)num7 / (float)TileYCount);
	}

	bool ILoadJson.Load(JsonData json)
	{
		MethodType = json["MethodType"].GETENUM(MethodType);
		StaticOffset = JsonTools.ToVector2(json["StaticOffset"], StaticOffset);
		StaticScale = JsonTools.ToVector2(json["StaticScale"], StaticScale);
		FrameCurve = JsonTools.ToCurve(json["FrameCurve"], FrameCurve);
		TileXCount = json["TileXCount"].GETINT(TileXCount);
		TileXUsing = json["TileXUsing"].GETINT(TileXUsing);
		TileYCount = json["TileYCount"].GETINT(TileYCount);
		TileYUsing = json["TileYUsing"].GETINT(TileYUsing);
		TileLoopCount = json["TileLoopCount"].GETINT(TileLoopCount);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldEnum("MethodType", MethodType);
		jsonData.AddField("StaticOffset", JsonTools.FromVector2(StaticOffset));
		jsonData.AddField("StaticScale", JsonTools.FromVector2(StaticScale));
		jsonData.AddField("FrameCurve", JsonTools.FromCurve(FrameCurve));
		jsonData.AddFieldLong("TileXCount", TileXCount);
		jsonData.AddFieldLong("TileXUsing", TileXUsing);
		jsonData.AddFieldLong("TileYCount", TileYCount);
		jsonData.AddFieldLong("TileYUsing", TileYUsing);
		jsonData.AddFieldLong("TileLoopCount", TileLoopCount);
		return jsonData;
	}
}
