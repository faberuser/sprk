using UnityEngine;

namespace NGameAsset;

public class CameraPlaneBounds
{
	public const int SIDE_N = 0;

	public const int SIDE_L = 1;

	public const int SIDE_R = 2;

	public const int SIDE_U = 3;

	public const int SIDE_D = 4;

	public Plane[] Planes = new Plane[5];

	public Vector3[] RayDir = new Vector3[5];

	public Vector3[] Pivot = new Vector3[5];

	public int Count => Planes.Length;

	public void CalcNearPivotPlane()
	{
		Vector3 normal = Planes[1].normal;
		_ = Planes[2].normal;
		Vector3 normal2 = Planes[3].normal;
		_ = Planes[4].normal;
		bool flag = false;
		Vector3 vector = Vector3.zero;
		Vector3 direction = Vector3.Cross(Vector3.right, normal2);
		Ray ray = new Ray(Pivot[3], direction);
		if (Planes[4].Raycast(ray, out var enter))
		{
			flag = true;
			vector = ray.GetPoint(enter);
		}
		bool flag2 = false;
		Vector3 vector2 = Vector3.zero;
		Vector3 direction2 = Vector3.Cross(Vector3.up, normal);
		Ray ray2 = new Ray(Pivot[1], direction2);
		if (Planes[2].Raycast(ray2, out var enter2))
		{
			flag2 = true;
			vector2 = ray2.GetPoint(enter2);
		}
		if (flag && flag2)
		{
			Vector3 vector3 = vector2 - vector;
			Pivot[0] = ((vector3.z < 0f) ? vector : vector2);
		}
		else if (flag)
		{
			Pivot[0] = vector;
		}
		else if (flag2)
		{
			Pivot[0] = vector2;
		}
		else
		{
			Pivot[0] = Vector3.back * 50f;
		}
		Planes[0] = new Plane(Vector3.forward, Pivot[0]);
	}
}
