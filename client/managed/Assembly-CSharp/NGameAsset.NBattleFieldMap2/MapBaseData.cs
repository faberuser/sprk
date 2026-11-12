using System.Collections.Generic;
using JsonBin;
using NGameAsset.NCreature;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class MapBaseData : ILoadJson
{
	public class FieldMapNodeTreeData : ILoadJson
	{
		public string PackPath = string.Empty;

		public string TreeCode = string.Empty;

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(PackPath))
			{
				return false;
			}
			if (string.IsNullOrEmpty(TreeCode))
			{
				return false;
			}
			return true;
		}

		bool ILoadJson.Load(JsonRead json)
		{
			PackPath = json["PackPath"].GETSTRING(PackPath);
			TreeCode = json["TreeCode"].GETSTRING(TreeCode);
			return true;
		}
	}

	public class FieldMapCreatureData : ILoadJson
	{
		public string Code = string.Empty;

		public int Star;

		public Vector3 Position = Vector3.zero;

		public Vector3 RotationEuler = Vector3.zero;

		public Vector3 Scale = Vector3.one;

		public AnimationKeyData AnimationKey = new AnimationKeyData(DefineCreature.EMotion.Idle, string.Empty);

		public DefineCreature.EMotionEnd MotionEnd;

		public Quaternion Rotation => Quaternion.Euler(RotationEuler);

		bool ILoadJson.Load(JsonRead json)
		{
			Code = json["Code"].GETSTRING(Code);
			Star = json["Star"].GETINT(Star);
			Position = json["Position"].GETVECTOR3(Position);
			RotationEuler = json["RotationEuler"].GETVECTOR3(RotationEuler);
			Scale = json["Scale"].GETVECTOR3(Scale);
			AnimationKey = AnimationKeyData.CreateFromJson(json["AnimationKey"]);
			MotionEnd = json["MotionEnd"].GETENUM(MotionEnd);
			return true;
		}
	}

	public class GuildFlagPivot : ILoadJson
	{
		public Vector3 Position = Vector3.zero;

		public Vector3 RotationEuler = Vector3.zero;

		public Vector3 Scale = Vector3.one;

		public Quaternion Rotation => Quaternion.Euler(RotationEuler);

		bool ILoadJson.Load(JsonRead json)
		{
			Position = json["Position"].GETVECTOR3(Position);
			RotationEuler = json["RotationEuler"].GETVECTOR3(RotationEuler);
			Scale = json["Scale"].GETVECTOR3(Scale);
			return true;
		}
	}

	public Vector3 BottomPivot = new Vector3(0f, 0f, -10f);

	public Vector3 BottomBillboardOffset = Vector3.zero;

	public Vector3 TopPivot = Vector3.zero;

	public Vector3 TopBillboardOffset = new Vector3(0f, 6f, 0f);

	public Vector3 LeftPivot = new Vector3(-1000f, 0f, 0f);

	public Vector3 LeftBillboardOffset = Vector3.zero;

	public Vector3 RightPivot = new Vector3(1000f, 0f, 0f);

	public Vector3 RightBillboardOffset = Vector3.zero;

	public List<GuildFlagPivot> GuildFlagForLeftList = new List<GuildFlagPivot>();

	public List<GuildFlagPivot> GuildFlagForRightList = new List<GuildFlagPivot>();

	public List<FieldMapCreatureData> FieldMapCreatureDataList = new List<FieldMapCreatureData>();

	public FieldMapNodeTreeData FieldMapNodeTree = new FieldMapNodeTreeData();

	bool ILoadJson.Load(JsonRead json)
	{
		if (json.Contains("GroundDepth"))
		{
			LoadOld(json);
			return true;
		}
		BottomPivot = json["BottomPivot"].GETVECTOR3(BottomPivot);
		BottomBillboardOffset = json["BottomBillboardOffset"].GETVECTOR3(BottomBillboardOffset);
		TopPivot = json["TopPivot"].GETVECTOR3(TopPivot);
		TopBillboardOffset = json["TopBillboardOffset"].GETVECTOR3(TopBillboardOffset);
		LeftPivot = json["LeftPivot"].GETVECTOR3(LeftPivot);
		LeftBillboardOffset = json["LeftBillboardOffset"].GETVECTOR3(LeftBillboardOffset);
		RightPivot = json["RightPivot"].GETVECTOR3(RightPivot);
		RightBillboardOffset = json["RightBillboardOffset"].GETVECTOR3(RightBillboardOffset);
		GuildFlagForLeftList.LoadListFromJson(json["GuildFlagForLeftList"]);
		GuildFlagForRightList.LoadListFromJson(json["GuildFlagForRightList"]);
		FieldMapCreatureDataList.LoadListFromJson(json["FieldMapCreatureDataList"]);
		FieldMapNodeTree.LOADFROMJSON(json["FieldMapNodeTree"]);
		return true;
	}

	private void LoadOld(JsonRead json)
	{
		float fLOAT = json["GroundDepth"].GETFLOAT(9.6f);
		float fLOAT2 = json["BackgroundHeight"].GETFLOAT(6f);
		float fLOAT3 = json["WidthMin"].GETFLOAT(-1000f);
		float fLOAT4 = json["WidthMax"].GETFLOAT(1000f);
		BottomPivot = new Vector3(0f, 0f, 0f - fLOAT);
		BottomBillboardOffset = Vector3.zero;
		TopPivot = Vector3.zero;
		TopBillboardOffset = new Vector3(0f, fLOAT2, 0f);
		LeftPivot = new Vector3(fLOAT3, 0f, 0f);
		LeftBillboardOffset = Vector3.zero;
		RightPivot = new Vector3(fLOAT4, 0f, 0f);
		RightBillboardOffset = Vector3.zero;
	}
}
