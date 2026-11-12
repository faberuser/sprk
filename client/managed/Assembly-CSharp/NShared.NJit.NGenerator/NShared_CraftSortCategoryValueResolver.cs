using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CraftSortCategoryValueResolver : IDataResolverT<CraftSortCategory>, IDataResolver
{
	private Type HandleType => typeof(CraftSortCategory);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(CraftSortCategory))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((CraftSortCategory)obj, jitSerializer);
	}

	public byte[] SerializeValue(CraftSortCategory value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(CraftSortCategory value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public CraftSortCategory Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (CraftSortCategory)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(CraftSortCategory target, ByteSpan span, JitPacker packer)
	{
	}

	public CraftSortCategory EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (CraftSortCategory)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
