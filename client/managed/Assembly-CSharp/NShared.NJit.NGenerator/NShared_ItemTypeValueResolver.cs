using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ItemTypeValueResolver : IDataResolverT<ItemType>, IDataResolver
{
	private Type HandleType => typeof(ItemType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(ItemType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((ItemType)obj, jitSerializer);
	}

	public byte[] SerializeValue(ItemType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(ItemType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public ItemType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (ItemType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(ItemType target, ByteSpan span, JitPacker packer)
	{
	}

	public ItemType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (ItemType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
