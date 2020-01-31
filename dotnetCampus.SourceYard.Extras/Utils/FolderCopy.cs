using System;
using System.IO;

namespace dotnetCampus.SourceYard.Extras.Utils
{
    internal static class FolderCopy
    {
        internal static void TransformFolderContents(string sourceFolder, string targetFolder,
            Func<FileInfo, string, string> contentTransformer = null)
        {
            var sourceDirectory = new DirectoryInfo(sourceFolder);
            foreach (var directory in sourceDirectory.EnumerateDirectories())
            {
                CopyFiles(directory.FullName, Path.Combine(targetFolder, directory.Name), SearchOption.AllDirectories, contentTransformer: contentTransformer);
            }

            CopyFiles(sourceDirectory.FullName, targetFolder, SearchOption.TopDirectoryOnly, contentTransformer: contentTransformer);
        }

        private static void CopyFiles(string sourceFolder, string targetFolder, SearchOption searchOption,
            Func<string, string> nameConverter = null, Func<FileInfo, string, string> contentTransformer = null)
        {
            foreach (var file in new DirectoryInfo(sourceFolder).EnumerateFiles("*", searchOption))
            {
                var relativePath = MakeRelativePath(sourceFolder, file.FullName);
                var targetFile = Path.GetFullPath(Path.Combine(targetFolder, relativePath));
                var directory = Path.GetDirectoryName(targetFile);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var fileName = Path.GetFileName(targetFile);
                if (nameConverter != null)
                {
                    fileName = nameConverter(fileName);
                    targetFile = Path.Combine(Path.GetDirectoryName(targetFile) ?? throw new InvalidOperationException(), fileName);
                }

                if (contentTransformer == null)
                {
                    File.Copy(file.FullName, targetFile, true);
                }
                else
                {
                    var text = File.ReadAllText(file.FullName);
                    text = contentTransformer(file, text);
                    File.WriteAllText(targetFile, text);
                }
            }
        }

        private static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException(nameof(fromPath));
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException(nameof(toPath));
            }

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                // 不是同一种路径，无法转换成相对路径。
                return toPath;
            }

            if (fromUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase)
                && !fromPath.EndsWith("/") && !fromPath.EndsWith("\\"))
            {
                // 如果是文件系统，则视来源路径为文件夹。
                fromUri = new Uri(fromPath + Path.DirectorySeparatorChar);
            }

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}