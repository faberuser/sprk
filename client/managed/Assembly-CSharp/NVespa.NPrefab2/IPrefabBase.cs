using System.Text;

namespace NVespa.NPrefab2;

public interface IPrefabBase
{
	PrefabKey Key { get; }

	void Close();

	void Dump(StringBuilder builder);
}
