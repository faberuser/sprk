using System;
using System.Collections.Generic;
using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public class DictionaryResolver<K, V> : IDataResolverT<Dictionary<K, V>>, IDataResolver
{
	Type IDataResolver.GetResolveType()
	{
		return typeof(Dictionary<K, V>);
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		return SerializeValue(obj as Dictionary<K, V>, jitSerializer);
	}

	public byte[] SerializeValue(Dictionary<K, V> dic, JitSerializer jitSerializer)
	{
		if (dic == null)
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		int num = 0;
		if (dic != null)
		{
			num = dic.Count;
		}
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack(num);
		if (num > 0)
		{
			IDataResolverT<K> dataResolver = Proxy<K>.DataResolver;
			IDataResolverT<V> dataResolver2 = Proxy<V>.DataResolver;
			foreach (KeyValuePair<K, V> item in dic)
			{
				byte[] bytes = dataResolver.SerializeReference(item.Key, jitSerializer);
				byte[] bytes2 = dataResolver2.SerializeReference(item.Value, jitSerializer);
				simpleBufferWriter.WriteBytes(bytes);
				simpleBufferWriter.WriteBytes(bytes2);
			}
		}
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(Dictionary<K, V> dic, JitSerializer jitSerializer)
	{
		return jitSerializer.SerializeReference(dic);
	}

	public Dictionary<K, V> Evaluate(ByteSpan span, JitPacker packer)
	{
		return new Dictionary<K, V>();
	}

	public void EvaluateLater(Dictionary<K, V> target, ByteSpan span, JitPacker packer)
	{
		IDataResolverT<K> dataResolver = Proxy<K>.DataResolver;
		IDataResolverT<V> dataResolver2 = Proxy<V>.DataResolver;
		ByteSpan nextSpan;
		int num = span.ReadByMsgPack_Int32(0, out nextSpan);
		for (int i = 0; i < num; i++)
		{
			ByteSpan nextSpan2;
			K key = dataResolver.EvaluateReference(nextSpan, packer, out nextSpan2);
			V value = dataResolver2.EvaluateReference(nextSpan2, packer, out nextSpan);
			target.Add(key, value);
		}
	}

	public Dictionary<K, V> EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return JitSerializer.EvaluateReference<Dictionary<K, V>>(span, packer, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		JitSerializer.SkipEvaluateReference(span, out nextSpan);
	}
}
