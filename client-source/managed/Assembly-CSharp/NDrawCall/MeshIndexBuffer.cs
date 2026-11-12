using System.Collections.Generic;

namespace NDrawCall;

public class MeshIndexBuffer
{
	public static class IndexPool
	{
		private static int[] _empty;

		private static Dictionary<int, int[]> _dic;

		public static int TotalIntCount { get; private set; }

		static IndexPool()
		{
			_dic = new Dictionary<int, int[]>();
			_empty = new int[6];
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_empty); i++)
			{
				_empty[i] = 0;
			}
		}

		public static void Clear()
		{
			lock (_dic)
			{
				_dic.Clear();
				TotalIntCount = 0;
			}
		}

		public static int[] GetIndices(int vertCount)
		{
			if (vertCount <= 0)
			{
				return _empty;
			}
			lock (_dic)
			{
				if (_dic.TryGetValue(vertCount, out var value))
				{
					return value;
				}
				int indexCountFromVertCount = GetIndexCountFromVertCount(vertCount);
				value = new int[indexCountFromVertCount];
				int num = vertCount / 4;
				for (int i = 0; i < num; i++)
				{
					int num2 = i * 4;
					int num3 = i * 6;
					value[num3] = num2;
					value[num3 + 1] = num2 + 1;
					value[num3 + 2] = num2 + 2;
					value[num3 + 3] = num2 + 2;
					value[num3 + 4] = num2 + 3;
					value[num3 + 5] = num2;
				}
				_dic.Add(vertCount, value);
				TotalIntCount += indexCountFromVertCount;
				return value;
			}
		}
	}

	public int IndexCount { get; private set; }

	public int VertCount { get; private set; }

	public int[] Buffer { get; private set; }

	public static int GetIndexCountFromVertCount(int vertCount)
	{
		return (vertCount >> 1) * 3;
	}

	public MeshIndexBuffer()
	{
		VertCount = 0;
		IndexCount = 0;
		Buffer = IndexPool.GetIndices(0);
	}

	public void SetVertCount(int newVertCount)
	{
		if (VertCount != newVertCount)
		{
			VertCount = newVertCount;
			IndexCount = GetIndexCountFromVertCount(newVertCount);
			Buffer = IndexPool.GetIndices(newVertCount);
		}
	}
}
