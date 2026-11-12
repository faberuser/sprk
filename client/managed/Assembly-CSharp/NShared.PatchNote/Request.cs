namespace NShared.PatchNote;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "lobby/patch_note";

	public int PatchNoteIndex { get; set; }

	public string Language { get; set; }

	public byte BeforeNext { get; set; }

	public bool IsRead { get; set; }
}
