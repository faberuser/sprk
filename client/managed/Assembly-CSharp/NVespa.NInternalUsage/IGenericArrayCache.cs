using System.Text;

namespace NVespa.NInternalUsage;

public interface IGenericArrayCache
{
	void OnSceneChanged();

	void DumpSummary(StringBuilder builder);
}
