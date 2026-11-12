using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipItemSetGroupTypeValueResolver : IDataResolverT<EquipItemSetGroupType>, IDataResolver
{
	private Type HandleType => typeof(EquipItemSetGroupType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(EquipItemSetGroupType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((EquipItemSetGroupType)obj, jitSerializer);
	}

	public byte[] SerializeValue(EquipItemSetGroupType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(EquipItemSetGroupType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public EquipItemSetGroupType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (EquipItemSetGroupType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(EquipItemSetGroupType target, ByteSpan span, JitPacker packer)
	{
	}

	public EquipItemSetGroupType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (EquipItemSetGroupType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
