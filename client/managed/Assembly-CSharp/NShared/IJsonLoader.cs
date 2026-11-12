using System;

namespace NShared;

public interface IJsonLoader
{
	void Load(string path, Action<object> onLoad);
}
