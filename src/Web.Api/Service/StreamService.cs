using System.Data;
using Web.Api.Model.Request;

namespace Web.Api.Service
{
	public class StreamService : IStreamService
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		public StreamService(IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
		}
		public async Task<string> CreateStream(RequestModel postData)
		{
			//#1d3b78
			// Ensure the Organizer folder exists
			string organizerFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, $"wwwroot/{postData.Name}");

			// Check if the HTML file already exists
			string filePath = Path.Combine(organizerFolderPath, $"index.html");
			if (Directory.Exists(organizerFolderPath))
			{
				throw new DuplicateNameException($"File for {organizerFolderPath} exists!");
			}
			Directory.CreateDirectory(organizerFolderPath);
			// Read the content of the template HTML file
			string templateFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "template", "index.html");
			string htmlContent = await File.ReadAllTextAsync(templateFilePath);

			// Replace placeholders in the template with the provided name
			htmlContent = htmlContent.Replace("{{NamePlaceholder}}", postData.Name);
			htmlContent = htmlContent.Replace("{{LogoUrl}}", postData.LogoUrl);

			if (!string.IsNullOrEmpty(postData.BannerColor))
			{
				htmlContent = htmlContent.Replace("{{BannerColor}}", postData.BannerColor);
			}

			CopyFilesRecursively(Path.Combine(_webHostEnvironment.ContentRootPath, "template"),
				organizerFolderPath);

			// Save the modified HTML content to the new file
			await File.WriteAllTextAsync(filePath, htmlContent);

			// Return the path to the newly created HTML file
			return filePath;
		}

		private void CopyFilesRecursively(string sourcePath, string targetPath)
		{
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
			{
				Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
			}

			//Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
			{
				File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
			}
		}
	}
}
