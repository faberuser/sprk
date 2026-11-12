namespace NShared.PatchNote;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int NewPatchNote { get; set; }

	public bool IsMore { get; set; }

	public PatchNoteInfo PatchNote { get; set; }
}
