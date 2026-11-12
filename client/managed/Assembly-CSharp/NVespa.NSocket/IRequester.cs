using System;
using NShared;

namespace NVespa.NSocket;

public interface IRequester
{
	ConnectState State { get; }

	void Request<TReq, TRes>(TReq data, Action<TRes> onResponse) where TReq : BaseRequest where TRes : BaseResponse;

	void Close();
}
