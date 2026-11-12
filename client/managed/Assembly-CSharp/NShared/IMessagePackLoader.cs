using System;

namespace NShared;

public interface IMessagePackLoader
{
	bool CheckFileExist(string path);

	void Load<T>(string path, Action<object> onLoad);
}
