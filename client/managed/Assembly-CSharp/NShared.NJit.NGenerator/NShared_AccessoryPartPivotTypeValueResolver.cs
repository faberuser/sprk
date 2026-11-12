using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_AccessoryPartPivotTypeValueResolver : IDataResolverT<AccessoryPartPivotType>, IDataResolver
{
	private Type HandleType => typeof(AccessoryPartPivotType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(AccessoryPartPivotType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((AccessoryPartPivotType)obj, jitSerializer);
	}

	public byte[] SerializeValue(AccessoryPartPivotType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(AccessoryPartPivotType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public AccessoryPartPivotType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (AccessoryPartPivotType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(AccessoryPartPivotType target, ByteSpan span, JitPacker packer)
	{
	}

	public AccessoryPartPivotType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (AccessoryPartPivotType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
