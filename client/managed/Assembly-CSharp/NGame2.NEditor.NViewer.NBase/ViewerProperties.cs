using NVespa.NGui;

namespace NGame2.NEditor.NViewer.NBase;

public class ViewerProperties<TValue> : PropertyPage
{
	public ViewerProperties(IProvider<TValue> dataProvider)
		: base(dataProvider)
	{
	}

	private void Remove(int index)
	{
		if (_provider != null)
		{
			_provider.RemoveProperty(index);
		}
	}

	private void Reset()
	{
		if (_provider != null)
		{
			_provider.ResetProperties();
		}
	}

	public void DrawGUI()
	{
		using (new BlockManager().Horizontal())
		{
			DrawPropertyPageGUI("{0}.{1} 지우기");
			if (DrawHelper.ButtonNormal("지우기"))
			{
				int currenctPropertyInfoIndex = GetCurrenctPropertyInfoIndex();
				Remove(currenctPropertyInfoIndex);
			}
			if (DrawHelper.ButtonNormal("초기화"))
			{
				Reset();
			}
		}
	}
}
