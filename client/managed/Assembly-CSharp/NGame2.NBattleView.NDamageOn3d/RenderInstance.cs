using System.Collections.Generic;
using NDrawCall;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NDamageOn3d;

public class RenderInstance
{
	public enum EShaderType
	{
		Default,
		Critical,
		Heal,
		Block,
		Miss,
		Resist,
		Dot,
		__count__
	}

	public static float DisplayScale = 0.0025f;

	public float EndTime;

	public List<GeometryBuilder.GeometryData> pendingList = new List<GeometryBuilder.GeometryData>();

	public RenderingList<Vector3> verts = new RenderingList<Vector3>();

	public RenderingList<Vector2> layouts = new RenderingList<Vector2>();

	public RenderingList<Vector2> datas = new RenderingList<Vector2>();

	public RenderingList<Vector2> uvs = new RenderingList<Vector2>();

	public RenderingList<Color32> cols = new RenderingList<Color32>();

	private int _layer;

	private Mesh _mesh;

	private Material _material;

	public EShaderType ShaderType { get; private set; }

	public float CreateTime { get; private set; }

	public RenderInstance(EShaderType shaderType, Shader shader)
	{
		ShaderType = shaderType;
		CreateTime = GlobalTimer.time;
		_mesh = new Mesh();
		_mesh.MarkDynamic();
		if (SceneAutoBehaviour<GeometryBuilder>.instance.NguiAtlas != null)
		{
			_material = new Material(SceneAutoBehaviour<GeometryBuilder>.instance.NguiAtlas.spriteMaterial);
			_material.shader = shader;
			_material.renderQueue = 4700;
		}
		LayerInfo layerInfo = Game3DRoot.instance.GameCamera.GetLayerInfo(Game3DRoot.ELayerType.Default);
		_layer = layerInfo.Layer;
	}

	public void Reset()
	{
		GeometryBuilder.ReleaseGeometryList(pendingList);
		verts.ClearAndReleaseBuffer();
		layouts.ClearAndReleaseBuffer();
		datas.ClearAndReleaseBuffer();
		uvs.ClearAndReleaseBuffer();
		cols.ClearAndReleaseBuffer();
		if (_mesh != null)
		{
			_mesh.Clear();
		}
	}

	public void Release()
	{
		Reset();
		if (_mesh != null)
		{
			VespaObject.DestroyObject(_mesh);
			_mesh = null;
		}
	}

	public void Render(int renderQueue)
	{
		RefreshMesh();
		_material.renderQueue = renderQueue;
		Graphics.DrawMesh(_mesh, Matrix4x4.identity, _material, _layer, null, 0, RenderInstancePool.PropertyBlock);
	}

	private void RefreshMesh()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(pendingList))
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < pendingList.Count; i++)
		{
			num += pendingList[i].layouts.Count;
		}
		int num2 = layouts.Count;
		layouts.AddEmpty(num);
		Vector2[] buffer = layouts.buffer;
		verts.ReserveCount(num);
		datas.ReserveCount(num);
		uvs.ReserveCount(num);
		cols.ReserveCount(num);
		for (int j = 0; j < pendingList.Count; j++)
		{
			GeometryBuilder.GeometryData geometryData = pendingList[j];
			Vector3 position = geometryData.Position;
			Vector2 layoutOffset = geometryData.LayoutOffset;
			float num3 = geometryData.Scale * DisplayScale;
			float createTime = geometryData.CreateTime;
			float extraValue = geometryData.ExtraValue;
			int count = geometryData.layouts.Count;
			Vector2[] buffer2 = geometryData.layouts.buffer;
			for (int k = 0; k < count; k++)
			{
				Vector2 vector = buffer2[k];
				buffer[num2 + k] = new Vector2(vector.x * num3 + layoutOffset.x, vector.y * num3 + layoutOffset.y);
			}
			num2 += count;
			Vector2 item = new Vector2(createTime, extraValue);
			datas.AddCount(item, count);
			verts.AddCount(position, count);
			uvs.AddList(geometryData.uvs);
			cols.AddList(geometryData.cols);
		}
		int[] indices = MeshIndexBuffer.IndexPool.GetIndices(verts.Count);
		_mesh.Clear();
		_mesh.vertices = verts.buffer;
		_mesh.triangles = indices;
		_mesh.uv = uvs.buffer;
		_mesh.uv2 = layouts.buffer;
		_mesh.uv3 = datas.buffer;
		_mesh.colors32 = cols.buffer;
		GeometryBuilder.ReleaseGeometryList(pendingList);
	}
}
