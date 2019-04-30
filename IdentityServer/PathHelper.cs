namespace IdentityServer
{
	public static class PathHelper
	{
		public static string EndingSlash(this string path)
		{
			return path.EndsWith("/") ? path : path + "/";
		}

		public static string NoEndingSlash(this string path)
		{
			return path.EndsWith("/") ? path.Substring(0, path.Length - 1) : path;
		}
	}
}
