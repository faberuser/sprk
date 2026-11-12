using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UIAlphaGradationSprite : UISprite, IUIAlphaGradation
{
	public class VertexInfo
	{
		public Vector3 Vertex;

		public Vector2 UV;

		public Color32 Color;

		public VertexInfo(Vector3 vertex, Vector2 uv, Color32 color)
		{
			Vertex = vertex;
			UV = uv;
			Color = color;
		}
	}

	public enum VertexName
	{
		LB,
		LT,
		RT,
		RB,
		CLB,
		CLT,
		CRT,
		CRB
	}

	public class Coord : Tuple<int, int>
	{
		public Coord(int a, int b)
			: base(a, b)
		{
		}
	}

	[Range(0f, 0.5f)]
	[HideInInspector]
	[SerializeField]
	private float _allClipAmount;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _topClipAmount;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _bottomClipAmount = 1f;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _leftClipAmount;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	private float _rightClipAmount = 1f;

	private static List<VertexInfo> _tempVertextInfoList = new List<VertexInfo>();

	[ExecuteInEditMode]
	public float AllClipAmount
	{
		get
		{
			return _allClipAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_allClipAmount != num)
			{
				_allClipAmount = num;
				mChanged = true;
			}
		}
	}

	[ExecuteInEditMode]
	public float TopClipAmount
	{
		get
		{
			return _topClipAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_topClipAmount != num)
			{
				_topClipAmount = num;
				mChanged = true;
			}
		}
	}

	[ExecuteInEditMode]
	public float BottomClipAmount
	{
		get
		{
			return _bottomClipAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_bottomClipAmount != num)
			{
				_bottomClipAmount = num;
				mChanged = true;
			}
		}
	}

	[ExecuteInEditMode]
	public float LeftClipAmount
	{
		get
		{
			return _leftClipAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_leftClipAmount != num)
			{
				_leftClipAmount = num;
				mChanged = true;
			}
		}
	}

	[ExecuteInEditMode]
	public float RightClipAmount
	{
		get
		{
			return _rightClipAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_rightClipAmount != num)
			{
				_rightClipAmount = num;
				mChanged = true;
			}
		}
	}

	public override void OnFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		base.OnFill(verts, uvs, cols);
		FillGradation(_topClipAmount, _bottomClipAmount, _leftClipAmount, _rightClipAmount, verts, uvs, cols);
	}

	public static void FillGradation(float topClipAmount, float bottomClipAmount, float leftClipAmount, float rightClipAmount, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		if (verts != null && uvs != null && cols != null && verts.size >= 4 && uvs.size >= 4 && cols.size >= 4)
		{
			Vector3[] array = new Vector3[4]
			{
				verts[0],
				verts[1],
				verts[2],
				verts[3]
			};
			Vector2[] array2 = new Vector2[4]
			{
				uvs[0],
				uvs[1],
				uvs[2],
				uvs[3]
			};
			Color32[] obj = new Color32[4]
			{
				cols[0],
				cols[1],
				cols[2],
				cols[3]
			};
			verts.Clear();
			uvs.Clear();
			cols.Clear();
			Vector3 vertex = array[0];
			Vector3 vertex2 = array[1];
			Vector3 vertex3 = array[2];
			Vector3 vertex4 = array[3];
			Vector2 uv = array2[0];
			Vector2 uv2 = array2[1];
			Vector2 uv3 = array2[2];
			Vector2 uv4 = array2[3];
			Color32 color = obj[0];
			Color32 color2 = obj[0];
			color2.a = 0;
			float y = Mathf.Lerp(vertex2.y, vertex4.y, topClipAmount);
			float y2 = Mathf.Lerp(vertex2.y, vertex4.y, bottomClipAmount);
			float x = Mathf.Lerp(vertex2.x, vertex4.x, leftClipAmount);
			float x2 = Mathf.Lerp(vertex2.x, vertex4.x, rightClipAmount);
			float y3 = Mathf.Lerp(uv2.y, uv4.y, topClipAmount);
			float y4 = Mathf.Lerp(uv2.y, uv4.y, bottomClipAmount);
			float x3 = Mathf.Lerp(uv2.x, uv4.x, leftClipAmount);
			float x4 = Mathf.Lerp(uv2.x, uv4.x, rightClipAmount);
			Vector3 vertex5 = new Vector3(x, y, 0f);
			Vector3 vertex6 = new Vector3(x2, y, 0f);
			Vector3 vertex7 = new Vector3(x2, y2, 0f);
			Vector3 vertex8 = new Vector3(x, y2, 0f);
			Vector2 uv5 = new Vector2(x3, y3);
			Vector2 uv6 = new Vector2(x4, y3);
			Vector2 uv7 = new Vector2(x4, y4);
			Vector2 uv8 = new Vector2(x3, y4);
			_tempVertextInfoList.Clear();
			_tempVertextInfoList.Add(new VertexInfo(vertex, uv, color2));
			_tempVertextInfoList.Add(new VertexInfo(vertex2, uv2, color2));
			_tempVertextInfoList.Add(new VertexInfo(vertex3, uv3, color2));
			_tempVertextInfoList.Add(new VertexInfo(vertex4, uv4, color2));
			_tempVertextInfoList.Add(new VertexInfo(vertex8, uv8, color));
			_tempVertextInfoList.Add(new VertexInfo(vertex5, uv5, color));
			_tempVertextInfoList.Add(new VertexInfo(vertex6, uv6, color));
			_tempVertextInfoList.Add(new VertexInfo(vertex7, uv7, color));
			SetRectVertex(new int[4] { 4, 5, 6, 7 }, _tempVertextInfoList, verts, uvs, cols);
			SetRectVertex(new int[4] { 5, 1, 2, 6 }, _tempVertextInfoList, verts, uvs, cols);
			SetRectVertex(new int[4] { 0, 4, 7, 3 }, _tempVertextInfoList, verts, uvs, cols);
			SetRectVertex(new int[4] { 0, 1, 5, 4 }, _tempVertextInfoList, verts, uvs, cols);
			SetRectVertex(new int[4] { 7, 6, 2, 3 }, _tempVertextInfoList, verts, uvs, cols);
		}
	}

	private static void SetRectVertex(int[] vertexIndices, List<VertexInfo> _tempVertextInfoList, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		foreach (int index in vertexIndices)
		{
			VertexInfo vertexInfo = _tempVertextInfoList[index];
			verts.AddUnsafe(vertexInfo.Vertex);
			uvs.AddUnsafe(vertexInfo.UV);
			cols.AddUnsafe(vertexInfo.Color);
		}
	}

	private static void SetRectVertex(List<VertexInfo> tempVertextInfoList, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		foreach (VertexInfo tempVertextInfo in tempVertextInfoList)
		{
			verts.AddUnsafe(tempVertextInfo.Vertex);
			uvs.AddUnsafe(tempVertextInfo.UV);
			cols.AddUnsafe(tempVertextInfo.Color);
		}
	}
}
