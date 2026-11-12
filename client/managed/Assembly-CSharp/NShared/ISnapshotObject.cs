using System.Collections.Generic;

namespace NShared;

public interface ISnapshotObject
{
	SnapshotRefCounter RefCounter { get; }

	ISnapshotObjectPool Pool { get; set; }

	bool PendingRelease { get; set; }

	object Serialize(SerializedSnapshotContainer container);

	void Deserialize(object s);

	string Dump();

	void Parse(IList<string> dumped, DumpedSnapshotContainer container);

	long GetUid();

	void SetUid(long uid);

	string GetTypeName();

	void OnRelease();

	IList<ISnapshotObject> GetChildren();
}
