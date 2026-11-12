using System.Collections.Generic;
using System.Reflection;

namespace NGame2.NEditor.NViewer;

public interface IProvider
{
	int GetListCount();

	PropertyInfo[] GeCachedtPropertyInfos();

	PropertyInfo GetPropertyInfo(int index);

	void RemoveProperty(int index);

	void ResetProperties();
}
public interface IProvider<T> : IProvider
{
	List<T> GetList();
}
