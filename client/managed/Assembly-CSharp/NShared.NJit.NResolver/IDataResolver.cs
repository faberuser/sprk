using System;

namespace NShared.NJit.NResolver;

public interface IDataResolver
{
	Type GetResolveType();

	byte[] SerializeObject(object obj, JitSerializer jitSerializer);
}
