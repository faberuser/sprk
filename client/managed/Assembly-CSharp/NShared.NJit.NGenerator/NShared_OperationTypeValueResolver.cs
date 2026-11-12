using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_OperationTypeValueResolver : IDataResolverT<OperationType>, IDataResolver
{
	private Type HandleType => typeof(OperationType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(OperationType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((OperationType)obj, jitSerializer);
	}

	public byte[] SerializeValue(OperationType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(OperationType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public OperationType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (OperationType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(OperationType target, ByteSpan span, JitPacker packer)
	{
	}

	public OperationType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (OperationType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
