using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ItemData_GetEffectTypeValueResolver : IDataResolverT<ItemData.GetEffectType>, IDataResolver
{
	private Type HandleType => typeof(ItemData.GetEffectType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(ItemData.GetEffectType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((ItemData.GetEffectType)obj, jitSerializer);
	}

	public byte[] SerializeValue(ItemData.GetEffectType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(ItemData.GetEffectType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public ItemData.GetEffectType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (ItemData.GetEffectType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(ItemData.GetEffectType target, ByteSpan span, JitPacker packer)
	{
	}

	public ItemData.GetEffectType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (ItemData.GetEffectType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
