using System.Collections.Generic;
using NVespa;

namespace NGame2.NAccount;

public static class UserObserverManagerContainer
{
	public static List<WeakRef<IUserObserverManager>> ManagerList = new List<WeakRef<IUserObserverManager>>();
}
