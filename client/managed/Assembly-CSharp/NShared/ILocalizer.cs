using System;

namespace NShared;

public interface ILocalizer
{
	string GetString(Enum type, params object[] values);
}
