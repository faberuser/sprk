using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_InventoryTypeValueResolver : IDataResolverT<InventoryType>, IDataResolver
{
	private Type HandleType => typeof(InventoryType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(InventoryType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((InventoryType)obj, jitSerializer);
	}

	public byte[] SerializeValue(InventoryType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(InventoryType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public InventoryType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (InventoryType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(InventoryType target, ByteSpan span, JitPacker packer)
	{
	}

	public InventoryType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (InventoryType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
