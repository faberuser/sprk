using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ContentsStatusTypeValueResolver : IDataResolverT<ContentsStatusType>, IDataResolver
{
	private Type HandleType => typeof(ContentsStatusType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(ContentsStatusType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((ContentsStatusType)obj, jitSerializer);
	}

	public byte[] SerializeValue(ContentsStatusType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(ContentsStatusType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public ContentsStatusType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (ContentsStatusType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(ContentsStatusType target, ByteSpan span, JitPacker packer)
	{
	}

	public ContentsStatusType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (ContentsStatusType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
