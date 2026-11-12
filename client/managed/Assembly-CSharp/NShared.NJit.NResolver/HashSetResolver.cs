using System;
using System.Collections.Generic;
using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public class HashSetResolver<TItem> : IDataResolverT<HashSet<TItem>>, IDataResolver
{
	Type IDataResolver.GetResolveType()
	{
		return typeof(HashSet<TItem>);
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		return SerializeValue(obj as HashSet<TItem>, jitSerializer);
	}

	public byte[] SerializeValue(HashSet<TItem> hashSet, JitSerializer jitSerializer)
	{
		if (hashSet == null)
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		int num = 0;
		if (hashSet != null)
		{
			num = hashSet.Count;
		}
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack(num);
		if (num > 0)
		{
			foreach (TItem item in hashSet)
			{
				byte[] bytes = Proxy<TItem>.DataResolver.SerializeReference(item, jitSerializer);
				simpleBufferWriter.WriteBytes(bytes);
			}
		}
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(HashSet<TItem> hashSet, JitSerializer jitSerializer)
	{
		return jitSerializer.SerializeReference(hashSet);
	}

	public HashSet<TItem> Evaluate(ByteSpan span, JitPacker packer)
	{
		return new HashSet<TItem>();
	}

	public void EvaluateLater(HashSet<TItem> target, ByteSpan span, JitPacker packer)
	{
		IDataResolverT<TItem> dataResolver = Proxy<TItem>.DataResolver;
		ByteSpan nextSpan;
		int num = span.ReadByMsgPack_Int32(0, out nextSpan);
		for (int i = 0; i < num; i++)
		{
			TItem item = dataResolver.EvaluateReference(nextSpan, packer, out nextSpan);
			target.Add(item);
		}
	}

	public HashSet<TItem> EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return JitSerializer.EvaluateReference<HashSet<TItem>>(span, packer, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		JitSerializer.SkipEvaluateReference(span, out nextSpan);
	}
}
