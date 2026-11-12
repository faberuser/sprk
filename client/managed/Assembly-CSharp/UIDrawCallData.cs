using System.Collections.Generic;
using NVespa;
using UnityEngine;

public class UIDrawCallData
{
	public struct PendingList<T> where T : struct
	{
		public RenderingList<T> List;

		public int Count;
	}

	public struct PendingCount<T>
	{
		public T Value;

		public int Count;
	}

	public List<PendingList<Vector3>> vertsPending = new List<PendingList<Vector3>>();

	public List<PendingList<Vector2>> uvsPending = new List<PendingList<Vector2>>();

	public List<PendingList<Color32>> colsPending = new List<PendingList<Color32>>();

	public List<PendingCount<Vector3>> normsPending = new List<PendingCount<Vector3>>();

	public List<PendingCount<Vector4>> tansPending = new List<PendingCount<Vector4>>();

	public RenderingList<Vector3> verts = new RenderingList<Vector3>();

	public RenderingList<Vector2> uvs = new RenderingList<Vector2>();

	public RenderingList<Color32> cols = new RenderingList<Color32>();

	public RenderingList<Vector3> norms = new RenderingList<Vector3>();

	public RenderingList<Vector4> tans = new RenderingList<Vector4>();

	public bool IsEmpty
	{
		get
		{
			if (vertsPending.Count == 0)
			{
				return verts.Count == 0;
			}
			return false;
		}
	}

	public void ClearAll()
	{
		vertsPending.Clear();
		uvsPending.Clear();
		colsPending.Clear();
		normsPending.Clear();
		tansPending.Clear();
		verts.ClearAndReleaseBuffer();
		uvs.ClearAndReleaseBuffer();
		cols.ClearAndReleaseBuffer();
		norms.ClearAndReleaseBuffer();
		tans.ClearAndReleaseBuffer();
	}

	public void FlushPending()
	{
		FlushPendingList(vertsPending, verts);
		FlushPendingList(uvsPending, uvs);
		FlushPendingList(colsPending, cols);
		FlushPendingCount(normsPending, norms);
		FlushPendingCount(tansPending, tans);
	}

	private void FlushPendingList<T>(List<PendingList<T>> pendingList, RenderingList<T> targetList) where T : struct
	{
		int num = 0;
		int count = pendingList.Count;
		for (int i = 0; i < count; i++)
		{
			num += pendingList[i].Count;
		}
		if (num > 0)
		{
			targetList.ReserveCount(num);
			for (int j = 0; j < count; j++)
			{
				PendingList<T> pendingList2 = pendingList[j];
				targetList.AddList(pendingList2.List, pendingList2.Count);
			}
		}
		pendingList.Clear();
	}

	private void FlushPendingCount<T>(List<PendingCount<T>> pendingList, RenderingList<T> targetList) where T : struct
	{
		int num = 0;
		int count = pendingList.Count;
		for (int i = 0; i < count; i++)
		{
			num += pendingList[i].Count;
		}
		if (num > 0)
		{
			targetList.ReserveCount(num);
			for (int j = 0; j < count; j++)
			{
				PendingCount<T> pendingCount = pendingList[j];
				targetList.AddCount(pendingCount.Value, pendingCount.Count);
			}
		}
		pendingList.Clear();
	}

	public void PendingVerts(RenderingList<Vector3> list, int count)
	{
		vertsPending.Add(new PendingList<Vector3>
		{
			List = list,
			Count = count
		});
	}

	public void PendingUvs(RenderingList<Vector2> list, int count)
	{
		uvsPending.Add(new PendingList<Vector2>
		{
			List = list,
			Count = count
		});
	}

	public void PendingCols(RenderingList<Color32> list, int count)
	{
		colsPending.Add(new PendingList<Color32>
		{
			List = list,
			Count = count
		});
	}

	public void PendingNormsCount(Vector3 value, int count)
	{
		normsPending.Add(new PendingCount<Vector3>
		{
			Value = value,
			Count = count
		});
	}

	public void PendingTans(Vector4 value, int count)
	{
		tansPending.Add(new PendingCount<Vector4>
		{
			Value = value,
			Count = count
		});
	}
}
