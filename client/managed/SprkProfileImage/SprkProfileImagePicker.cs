using System;
using System.Runtime.InteropServices;

/// <summary>
/// The Windows "Open" dialog (comdlg32). Only called when
/// <see cref="SprkProfileImage.CanPickFiles"/> is true: the native imports bind
/// when a calling method is prepared, so Android never reaches this class.
/// </summary>
internal static class SprkProfileImagePicker
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct OpenFileName
	{
		public int lStructSize;

		public IntPtr hwndOwner;

		public IntPtr hInstance;

		public IntPtr lpstrFilter;

		public IntPtr lpstrCustomFilter;

		public int nMaxCustFilter;

		public int nFilterIndex;

		public IntPtr lpstrFile;

		public int nMaxFile;

		public IntPtr lpstrFileTitle;

		public int nMaxFileTitle;

		public IntPtr lpstrInitialDir;

		public IntPtr lpstrTitle;

		public int Flags;

		public short nFileOffset;

		public short nFileExtension;

		public IntPtr lpstrDefExt;

		public IntPtr lCustData;

		public IntPtr lpfnHook;

		public IntPtr lpTemplateName;

		public IntPtr pvReserved;

		public int dwReserved;

		public int FlagsEx;
	}

	private const int OFN_NOCHANGEDIR = 0x8;

	private const int OFN_PATHMUSTEXIST = 0x800;

	private const int OFN_FILEMUSTEXIST = 0x1000;

	private const int OFN_EXPLORER = 0x80000;

	private const int MaxPath = 4096;

	[DllImport("comdlg32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetOpenFileNameW", SetLastError = true)]
	private static extern bool GetOpenFileName(ref OpenFileName ofn);

	[DllImport("user32.dll")]
	private static extern IntPtr GetActiveWindow();

	/// <summary>The chosen file's path, or null when cancelled. Blocks until the dialog closes.</summary>
	public static string PickWindows(string title)
	{
		// Each filter is "label\0patterns\0"; the list ends with an extra \0.
		IntPtr intPtr = Marshal.StringToHGlobalUni("Images (*.png;*.jpg;*.jpeg)\0*.png;*.jpg;*.jpeg\0\0");
		IntPtr intPtr2 = Marshal.StringToHGlobalUni(title ?? string.Empty);
		IntPtr intPtr3 = Marshal.AllocHGlobal(MaxPath * 2);
		try
		{
			Marshal.Copy(new byte[MaxPath * 2], 0, intPtr3, MaxPath * 2);
			OpenFileName ofn = new OpenFileName
			{
				lStructSize = Marshal.SizeOf(typeof(OpenFileName)),
				hwndOwner = GetActiveWindow(),
				lpstrFilter = intPtr,
				nFilterIndex = 1,
				lpstrFile = intPtr3,
				nMaxFile = MaxPath,
				lpstrTitle = intPtr2,
				Flags = OFN_NOCHANGEDIR | OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST | OFN_EXPLORER
			};
			if (!GetOpenFileName(ref ofn))
			{
				return null;
			}
			string text = Marshal.PtrToStringUni(intPtr3);
			return string.IsNullOrEmpty(text) ? null : text;
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
			Marshal.FreeHGlobal(intPtr2);
			Marshal.FreeHGlobal(intPtr3);
		}
	}
}
