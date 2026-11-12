using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature.NAnimationSample;

public class MeshRawData
{
	public class Factory : SingletonAuto<Factory>
	{
		public class PoolingData
		{
			public Queue<MeshRawData> Unused = new Queue<MeshRawData>();

			public int CreateNew;
		}

		private const int MinReserveCount = 16;

		private IncreaseDic<int, PoolingData> _dic = new IncreaseDic<int, PoolingData>();

		public void DestroyOnSceneChanged()
		{
			_dic.Clear();
		}

		public void ReserveOnSceneChanged()
		{
			ReserveUnused();
		}

		private void ReserveUnused()
		{
			int num = 16;
			for (int i = 0; i < 5; i++)
			{
				if (num >= 256)
				{
					break;
				}
				ReserveUnused(num, 32);
				num *= 2;
			}
		}

		private void ReserveUnused(int reserveCount, int unusedCount)
		{
			PoolingData pooling = GetPooling(reserveCount);
			for (int i = 0; i < unusedCount; i++)
			{
				if (pooling.Unused.Count >= unusedCount)
				{
					break;
				}
				pooling.CreateNew++;
				pooling.Unused.Enqueue(new MeshRawData(reserveCount));
			}
		}

		public MeshRawData Create(int trailPointCount)
		{
			int reserveCount = GetReserveCount(trailPointCount);
			PoolingData pooling = GetPooling(reserveCount);
			if (pooling.Unused.Count > 0)
			{
				return pooling.Unused.Dequeue();
			}
			pooling.CreateNew++;
			return new MeshRawData(reserveCount);
		}

		public void Release(MeshRawData meshRaw)
		{
			GetPooling(meshRaw.ReserveCount).Unused.Enqueue(meshRaw);
		}

		private PoolingData GetPooling(int reserveCount)
		{
			if (!_dic.TryGetValue(reserveCount, out var o_value))
			{
				o_value = new PoolingData();
				_dic.Add(reserveCount, o_value);
			}
			return o_value;
		}

		private int GetReserveCount(int trailPointCount)
		{
			int num;
			for (num = 16; num < trailPointCount; num *= 2)
			{
			}
			return num;
		}
	}

	public static readonly Color InitColor = Color.white;

	public static readonly Vector2 UV_0 = new Vector2(0f, 1f);

	public static readonly Vector2 UV_1 = new Vector2(0f, 0f);

	public static readonly Vector2 UV_2 = new Vector2(0f, 0.5f);

	public static int MaxTrailPointCount = 0;

	public int ReserveCount;

	public Vector3[] verticies;

	public Vector2[] uvs;

	public Color[] colors;

	public int[] indices;

	public int prevTrailPointCount;

	public MeshRawData(int reserveCount)
	{
		ReserveCount = reserveCount;
		int num = ReserveCount * 3 - 1;
		verticies = new Vector3[num];
		uvs = new Vector2[num];
		colors = new Color[num];
		indices = new int[(ReserveCount - 1) * 12];
		prevTrailPointCount = 1;
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = InitColor;
		}
		for (int j = 0; j < ReserveCount - 1; j++)
		{
			uvs[j * 3] = UV_0;
			uvs[j * 3 + 1] = UV_1;
			uvs[j * 3 + 2] = UV_2;
		}
		uvs[(ReserveCount - 1) * 3] = UV_0;
		uvs[(ReserveCount - 1) * 3 + 1] = UV_1;
		for (int k = 0; k < indices.Length; k++)
		{
			indices[k] = 0;
		}
	}

	public void FillIndices(int trailPointCount)
	{
		MaxTrailPointCount = Mathf.Max(MaxTrailPointCount, trailPointCount);
		if (prevTrailPointCount == trailPointCount)
		{
			return;
		}
		if (prevTrailPointCount < trailPointCount)
		{
			for (int i = prevTrailPointCount - 1; i < trailPointCount - 1; i++)
			{
				indices[i * 12] = i * 3 + 2;
				indices[i * 12 + 1] = i * 3;
				indices[i * 12 + 2] = i * 3 + 1;
				indices[i * 12 + 3] = i * 3 + 2;
				indices[i * 12 + 4] = i * 3 + 1;
				indices[i * 12 + 5] = i * 3 + 4;
				indices[i * 12 + 6] = i * 3 + 2;
				indices[i * 12 + 7] = i * 3 + 4;
				indices[i * 12 + 8] = i * 3 + 3;
				indices[i * 12 + 9] = i * 3 + 2;
				indices[i * 12 + 10] = i * 3 + 3;
				indices[i * 12 + 11] = i * 3;
			}
		}
		else
		{
			int num = (trailPointCount - 1) * 12;
			int num2 = (prevTrailPointCount - 1) * 12;
			for (int j = num; j < num2; j++)
			{
				indices[j] = 0;
			}
		}
		prevTrailPointCount = trailPointCount;
	}
}
