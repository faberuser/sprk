namespace NShared.RestoreArtifact;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/restore_artifact";

	public int ArtifactRestoreIndex { get; set; }
}
