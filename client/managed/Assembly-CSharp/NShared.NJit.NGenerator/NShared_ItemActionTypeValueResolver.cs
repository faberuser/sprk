using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ItemActionTypeValueResolver : IDataResolverT<ItemActionType>, IDataResolver
{
	private Type HandleType => typeof(ItemActionType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(ItemActionType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((ItemActionType)obj, jitSerializer);
	}

	public byte[] SerializeValue(ItemActionType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(ItemActionType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public ItemActionType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (ItemActionType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(ItemActionType target, ByteSpan span, JitPacker packer)
	{
	}

	public ItemActionType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (ItemActionType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
