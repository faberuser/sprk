using System.Collections.Generic;

namespace NShared;

public interface IDataElement<T> where T : DateElementField
{
	List<T> GetElementList();
}
