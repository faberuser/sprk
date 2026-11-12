using System.Text;

namespace NVespa;

public interface IReservePool
{
	void GetDescription(StringBuilder sb);

	void ClearUnused();

	void Reserve(bool resetAll);
}
