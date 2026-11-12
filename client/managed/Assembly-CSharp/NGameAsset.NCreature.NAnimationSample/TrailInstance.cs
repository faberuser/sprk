using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature.NAnimationSample;

public class TrailInstance
{
	public class TrailPoint
	{
		public Vector3 Top;

		public Vector3 Bottom;

		public float CreateFrame;

		public float CreateTime;

		public float EndTime;
	}

	public CircularBuffer<AnimationSampleData.SampleFrame> SampleBuffer;

	public CircularBuffer<TrailPoint> PointBuffer;

	public Mesh Mesh;

	public MeshRawData RawData;

	public TrailInstance(int sampleCount)
	{
		SampleBuffer = new CircularBuffer<AnimationSampleData.SampleFrame>(sampleCount);
		PointBuffer = new CircularBuffer<TrailPoint>(sampleCount);
		Mesh = new Mesh();
		Mesh.MarkDynamic();
	}

	public void Release()
	{
		if (Mesh != null)
		{
			VespaObject.DestroyObject(Mesh);
			Mesh = null;
		}
	}

	public void Clear()
	{
		for (int i = 0; i < SampleBuffer.Count; i++)
		{
			if (SampleBuffer[i].IsDynamic)
			{
				ReservePoolUtil.Release(SampleBuffer[i]);
			}
		}
		SampleBuffer.Clear();
		for (int j = 0; j < PointBuffer.Count; j++)
		{
			ReservePoolUtil.Release(PointBuffer[j]);
		}
		PointBuffer.Clear();
		Mesh.Clear();
		if (RawData != null)
		{
			SingletonAuto<MeshRawData.Factory>.instance.Release(RawData);
			RawData = null;
		}
	}

	public void AddSampleFrame(AnimationSampleData.SampleFrame sample)
	{
		SampleBuffer.Add(sample);
	}

	public void ReserveMeshRawData(int trailPointCount)
	{
		if (RawData != null)
		{
			if (trailPointCount <= RawData.ReserveCount)
			{
				return;
			}
			SingletonAuto<MeshRawData.Factory>.instance.Release(RawData);
		}
		RawData = SingletonAuto<MeshRawData.Factory>.instance.Create(trailPointCount);
	}

	public void BuildMesh()
	{
		Mesh.Clear();
		Mesh.vertices = RawData.verticies;
		Mesh.SetIndices(RawData.indices, MeshTopology.Triangles, 0);
		Mesh.uv = RawData.uvs;
		Mesh.colors = RawData.colors;
	}
}
