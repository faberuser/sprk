using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PortalRenewalCategoryValueResolver : IDataResolverT<PortalRenewalCategory>, IDataResolver
{
	private Type HandleType => typeof(PortalRenewalCategory);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(PortalRenewalCategory))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((PortalRenewalCategory)obj, jitSerializer);
	}

	public byte[] SerializeValue(PortalRenewalCategory value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(PortalRenewalCategory value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public PortalRenewalCategory Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (PortalRenewalCategory)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(PortalRenewalCategory target, ByteSpan span, JitPacker packer)
	{
	}

	public PortalRenewalCategory EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (PortalRenewalCategory)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
