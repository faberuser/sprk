using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ERightMenuButtonValueResolver : IDataResolverT<ERightMenuButton>, IDataResolver
{
	private Type HandleType => typeof(ERightMenuButton);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(ERightMenuButton))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((ERightMenuButton)obj, jitSerializer);
	}

	public byte[] SerializeValue(ERightMenuButton value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(ERightMenuButton value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public ERightMenuButton Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (ERightMenuButton)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(ERightMenuButton target, ByteSpan span, JitPacker packer)
	{
	}

	public ERightMenuButton EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (ERightMenuButton)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
