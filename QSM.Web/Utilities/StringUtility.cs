namespace QSM.Web.Utilities;

public static class StringUtility
{
	public static string SanitizeFileName(string folderName)
	{
		char[] invalidChars = Path.GetInvalidFileNameChars();

		if (folderName.IndexOfAny(invalidChars) == -1)
			return folderName;

		folderName = string.Join("", folderName.Split(invalidChars));

		if (folderName.Length == 0)
		{
			folderName = Path.GetRandomFileName();
		}

		return folderName;
	}
}