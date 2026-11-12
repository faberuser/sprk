using NGameAsset.NWorldMap;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class MapComponent
{
	public MapLayout Layout;

	public NodeGraph NodeGraph;

	public MapViewer Viewer;

	private ScaleData _scaleData;

	public ScaleData ScaleData
	{
		get
		{
			if (_scaleData != null)
			{
				return _scaleData;
			}
			return GameSetting.data.WorldMap.Scale;
		}
		set
		{
			_scaleData = value;
		}
	}
}
