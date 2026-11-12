using System;
using System.Collections.Generic;
using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public class JitDictionaryWithKeyResolver<K, V> : IDataResolverT<JitDictionaryWithKey<K, V>>, IDataResolver where V : IDataKeySelector<K>
{
	private class HashedData
	{
		public List<KeyValuePair<K, V>> pairList = new List<KeyValuePair<K, V>>();

		public byte[] Bytes;

		public int offset;
	}

	Type IDataResolver.GetResolveType()
	{
		return typeof(JitDictionaryWithKey<K, V>);
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		return SerializeValue(obj as JitDictionaryWithKey<K, V>, jitSerializer);
	}

	public byte[] SerializeValue(JitDictionaryWithKey<K, V> jitDic, JitSerializer jitSerializer)
	{
		if (jitDic == null)
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		Dictionary<K, V> dictionary = jitDic.JsonLoad_Get();
		int num = Math.Max(dictionary.Count * 2 / 10, 1);
		int num2 = dictionary.Count + num;
		HashedData[] hashedArray = GetHashedArray(dictionary, num2);
		IDataResolverT<V> valueResolver = jitDic.ValueResolver;
		for (int i = 0; i < hashedArray.Length; i++)
		{
			HashedData hashedData = hashedArray[i];
			hashedData.offset = 0;
			if (i > 0)
			{
				HashedData hashedData2 = hashedArray[i - 1];
				hashedData.offset = hashedData2.offset + hashedData2.Bytes.Length;
			}
			using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
			int count = hashedData.pairList.Count;
			simpleBufferWriter.WriteInt(count);
			for (int j = 0; j < count; j++)
			{
				byte[] bytes = valueResolver.SerializeReference(hashedData.pairList[j].Value, jitSerializer);
				simpleBufferWriter.WriteBytes(bytes);
			}
			hashedData.Bytes = simpleBufferWriter.ToArray();
			JitOption.Max_countInHashed_from_JitDictionary = Math.Max(count, JitOption.Max_countInHashed_from_JitDictionary);
		}
		using SimpleBufferWriter simpleBufferWriter2 = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter2.WriteInt(dictionary.Count);
		simpleBufferWriter2.WriteInt(num2);
		foreach (HashedData hashedData3 in hashedArray)
		{
			simpleBufferWriter2.WriteInt(hashedData3.offset);
		}
		foreach (HashedData hashedData4 in hashedArray)
		{
			simpleBufferWriter2.WriteBytes(hashedData4.Bytes);
		}
		return simpleBufferWriter2.ToArray();
	}

	public byte[] SerializeReference(JitDictionaryWithKey<K, V> jitDic, JitSerializer jitSerializer)
	{
		return jitSerializer.SerializeReference(jitDic);
	}

	public JitDictionaryWithKey<K, V> Evaluate(ByteSpan span, JitPacker packer)
	{
		JitDictionaryWithKey<K, V> jitDictionaryWithKey = new JitDictionaryWithKey<K, V>();
		((IJitContainer)jitDictionaryWithKey).Bind(packer);
		return jitDictionaryWithKey;
	}

	public void EvaluateLater(JitDictionaryWithKey<K, V> target, ByteSpan span, JitPacker packer)
	{
		((IJitContainer)target).Deserialize(span);
	}

	public JitDictionaryWithKey<K, V> EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return JitSerializer.EvaluateReference<JitDictionaryWithKey<K, V>>(span, packer, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		JitSerializer.SkipEvaluateReference(span, out nextSpan);
	}

	private HashedData[] GetHashedArray(Dictionary<K, V> dic, int modCount)
	{
		IKeyComparerT<K> comparer = ResolverContainer.GetComparer<K>();
		HashedData[] array = new HashedData[modCount];
		for (int i = 0; i < modCount; i++)
		{
			array[i] = new HashedData();
		}
		foreach (KeyValuePair<K, V> item in dic)
		{
			int num = comparer.GetHashCode(item.Key) % modCount;
			if (num < 0)
			{
				num += modCount;
			}
			array[num].pairList.Add(item);
		}
		return array;
	}
}
