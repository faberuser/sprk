using System;

namespace NShared;

public interface ITextLoader
{
	void Load(string path, Action<string> onLoad);
}
