using System;

namespace NVespa.NSocket;

public interface IHandler
{
	void Register<T>(Action<T> handler) where T : class;

	void Clear();
}
