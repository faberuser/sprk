using System.Collections.Generic;

namespace NGame2.NEditor.NViewer;

public class ListDefaultViewer<TValue> : BaseGenericViewer<TValue>
{
	public ListDefaultViewer(IEnumerable<TValue> values)
	{
		Set(values);
	}
}
