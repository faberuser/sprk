using System.Collections.Generic;

namespace NShared;

public interface IOperationGroupCondition
{
	void Check(IList<ModifiedFiredOperationInfo> modifiedInfos);
}
