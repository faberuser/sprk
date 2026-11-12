using NGame2.NNodeTree;
using NGame2.NScene.NPreloader;

namespace NGame2.NUI.NRender3D.NCinema;

public interface INodeTreeLoadProvider
{
	PackLoad PackLoad { get; set; }

	TreeData TreeData { get; set; }

	SimplePreloader Preloader { get; }

	string NodeTreePack { get; }

	string NodeTreeName { get; }
}
