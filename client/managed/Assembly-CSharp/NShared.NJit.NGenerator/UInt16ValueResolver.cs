using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class UInt16ValueResolver : IDataResolverT<ushort>, IDataResolver
{
	private Type HandleType => typeof(ushort);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(ushort))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((ushort)obj, jitSerializer);
	}

	public byte[] SerializeValue(ushort value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteUInt16ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(ushort value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteUInt16ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public ushort Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return span.ReadByMsgPack_UInt16(0, out nextSpan);
	}

	public void EvaluateLater(ushort target, ByteSpan span, JitPacker packer)
	{
	}

	public ushort EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return span.ReadByMsgPack_UInt16(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_UInt16(0, out nextSpan);
	}
}
