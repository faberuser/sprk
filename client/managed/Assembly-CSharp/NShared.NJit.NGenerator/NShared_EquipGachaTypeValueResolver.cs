using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipGachaTypeValueResolver : IDataResolverT<EquipGachaType>, IDataResolver
{
	private Type HandleType => typeof(EquipGachaType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(EquipGachaType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((EquipGachaType)obj, jitSerializer);
	}

	public byte[] SerializeValue(EquipGachaType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(EquipGachaType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public EquipGachaType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (EquipGachaType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(EquipGachaType target, ByteSpan span, JitPacker packer)
	{
	}

	public EquipGachaType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (EquipGachaType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
