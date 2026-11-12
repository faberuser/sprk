using System;
using System.Collections.Generic;
using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public class JitListResolver<V> : IDataResolverT<JitList<V>>, IDataResolver
{
	Type IDataResolver.GetResolveType()
	{
		return typeof(JitList<V>);
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		return SerializeValue(obj as JitList<V>, jitSerializer);
	}

	public byte[] SerializeValue(JitList<V> jitList, JitSerializer jitSerializer)
	{
		if (jitList == null)
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		List<V> list = jitList.JsonLoad_GetOrNew();
		int count = list.Count;
		IDataResolverT<V> valueResolver = jitList.ValueResolver;
		List<byte[]> list2 = new List<byte[]>();
		for (int i = 0; i < count; i++)
		{
			V value = list[i];
			byte[] item = valueResolver.SerializeReference(value, jitSerializer);
			list2.Add(item);
		}
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt(count);
		int num = 0;
		for (int j = 0; j < count; j++)
		{
			simpleBufferWriter.WriteInt(num);
			num += list2[j].Length;
		}
		for (int k = 0; k < count; k++)
		{
			simpleBufferWriter.WriteBytes(list2[k]);
		}
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(JitList<V> jitList, JitSerializer jitSerializer)
	{
		return jitSerializer.SerializeReference(jitList);
	}

	public JitList<V> Evaluate(ByteSpan span, JitPacker packer)
	{
		JitList<V> jitList = new JitList<V>();
		((IJitContainer)jitList).Bind(packer);
		return jitList;
	}

	public void EvaluateLater(JitList<V> target, ByteSpan span, JitPacker packer)
	{
		((IJitContainer)target).Deserialize(span);
	}

	public JitList<V> EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return JitSerializer.EvaluateReference<JitList<V>>(span, packer, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		JitSerializer.SkipEvaluateReference(span, out nextSpan);
	}
}
