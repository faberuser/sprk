using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipItemSubTypeValueResolver : IDataResolverT<EquipItemSubType>, IDataResolver
{
	private Type HandleType => typeof(EquipItemSubType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(EquipItemSubType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((EquipItemSubType)obj, jitSerializer);
	}

	public byte[] SerializeValue(EquipItemSubType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(EquipItemSubType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public EquipItemSubType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (EquipItemSubType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(EquipItemSubType target, ByteSpan span, JitPacker packer)
	{
	}

	public EquipItemSubType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (EquipItemSubType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
