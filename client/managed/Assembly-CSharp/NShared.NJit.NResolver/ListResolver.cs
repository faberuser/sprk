using System;
using System.Collections.Generic;
using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public class ListResolver<TItem> : IDataResolverT<List<TItem>>, IDataResolver
{
	Type IDataResolver.GetResolveType()
	{
		return typeof(List<TItem>);
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		return SerializeValue(obj as List<TItem>, jitSerializer);
	}

	public byte[] SerializeValue(List<TItem> list, JitSerializer jitSerializer)
	{
		if (list == null)
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		int num = 0;
		if (list != null)
		{
			num = list.Count;
		}
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack(num);
		for (int i = 0; i < num; i++)
		{
			byte[] bytes = Proxy<TItem>.DataResolver.SerializeReference(list[i], jitSerializer);
			simpleBufferWriter.WriteBytes(bytes);
		}
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(List<TItem> list, JitSerializer jitSerializer)
	{
		return jitSerializer.SerializeReference(list);
	}

	public List<TItem> Evaluate(ByteSpan span, JitPacker packer)
	{
		return new List<TItem>();
	}

	public void EvaluateLater(List<TItem> target, ByteSpan span, JitPacker packer)
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

	public List<TItem> EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return JitSerializer.EvaluateReference<List<TItem>>(span, packer, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		JitSerializer.SkipEvaluateReference(span, out nextSpan);
	}
}
