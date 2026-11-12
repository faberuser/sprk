using System.Collections.Generic;

namespace NVespa.NTask;

public interface ITask
{
	string Name { get; set; }

	IEnumerator<TaskCurrentData> Run();
}
