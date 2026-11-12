using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_InspectUserTypeValueResolver : IDataResolverT<InspectUserType>, IDataResolver
{
	private Type HandleType => typeof(InspectUserType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(InspectUserType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((InspectUserType)obj, jitSerializer);
	}

	public byte[] SerializeValue(InspectUserType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(InspectUserType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public InspectUserType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (InspectUserType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(InspectUserType target, ByteSpan span, JitPacker packer)
	{
	}

	public InspectUserType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (InspectUserType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
