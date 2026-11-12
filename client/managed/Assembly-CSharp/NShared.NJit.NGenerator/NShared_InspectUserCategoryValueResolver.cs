using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_InspectUserCategoryValueResolver : IDataResolverT<InspectUserCategory>, IDataResolver
{
	private Type HandleType => typeof(InspectUserCategory);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(InspectUserCategory))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((InspectUserCategory)obj, jitSerializer);
	}

	public byte[] SerializeValue(InspectUserCategory value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(InspectUserCategory value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public InspectUserCategory Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (InspectUserCategory)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(InspectUserCategory target, ByteSpan span, JitPacker packer)
	{
	}

	public InspectUserCategory EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (InspectUserCategory)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
