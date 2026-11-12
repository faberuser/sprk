using System;
using PigeonCoopToolkit.Utillities;
using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

public class PCTrail : IDisposable
{
	public CircularBuffer<PCTrailPoint> Points;

	public Mesh Mesh;

	public Vector3[] verticies;

	public Vector3[] normals;

	public Vector2[] uvs;

	public Color[] colors;

	public int[] indicies;

	public int activePointCount;

	public bool IsActiveTrail;

	public PCTrail(int numPoints)
	{
		Mesh = new Mesh();
		Mesh.MarkDynamic();
		verticies = new Vector3[2 * numPoints];
		normals = new Vector3[2 * numPoints];
		uvs = new Vector2[2 * numPoints];
		colors = new Color[2 * numPoints];
		indicies = new int[2 * numPoints * 3];
		Points = new CircularBuffer<PCTrailPoint>(numPoints);
	}

	public void Dispose()
	{
		if (Mesh != null)
		{
			VespaObject.DestroyWithAssets(Mesh);
			Mesh = null;
		}
		Points.Clear();
		Points = null;
	}
}
