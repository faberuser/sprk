using System.Collections.Generic;

namespace NGame2.NEditor.NViewer;

public class BaseGenericViewer2<TValue> : BaseAbstarctViewer2<TValue>
{
	private object _origin;

	protected override void Refresh()
	{
		if (_origin != null)
		{
			Initilize();
			InitCachedList();
			SetCachedList(_origin as IEnumerable<TValue>);
		}
	}

	public override void Set(object datas)
	{
		_origin = datas;
	}
}
