using System.Data;
using Web.Api.Model.Request;
using Web.Api.Repository;

namespace Web.Api.Service
{
	public class StreamService : IStreamService
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IHttpContextAccessor _httpContextAccessor;
		//private readonly IDataRepository _dataRepository;
		public StreamService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment/*, IDataRepository dataRepository*/)
		{
			_httpContextAccessor = httpContextAccessor;
			_webHostEnvironment = webHostEnvironment;
			//_dataRepository = dataRepository;
		}
		public async Task<string> CreateStream(RequestModel postData)
		{
			//#1d3b78
			// Ensure the Organizer folder exists
			string organizerFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, $"wwwroot/{postData.Name}");

			var request = _httpContextAccessor.HttpContext.Request;

			var protocol = request.Scheme; // "http" or "https"
			var host = request.Host.Value;
			var pathBase = request.PathBase;

			// If you want to include the base path, use:
			// var fullPath = $"{protocol}://{host}{pathBase}";

			var fullPath = $"{protocol}://{host}/{postData.Name}/index.html";

			// Check if the HTML file already exists
			string filePath = Path.Combine(organizerFolderPath, $"index.html");
			if (Directory.Exists(organizerFolderPath))
			{
				throw new DuplicateNameException($"File for {fullPath} exists!");
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

			// Read the content of script.js from the template
			string scriptFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "template", "script.js");
			string scriptContent = await File.ReadAllTextAsync(scriptFilePath);

			// Replace "WeTech" with postData.Name in script.js
			scriptContent = scriptContent.Replace("WeTech", postData.Name);
			scriptContent = scriptContent.Replace("wetech", postData.Name);

			CopyFilesRecursively(Path.Combine(_webHostEnvironment.ContentRootPath, "template"),
				organizerFolderPath);

			// Save the modified HTML content to the new file
			await File.WriteAllTextAsync(filePath, htmlContent);

			// Save the modified script.js to the organizer folder
			string scriptPath = Path.Combine(organizerFolderPath, "script.js");
			await File.WriteAllTextAsync(scriptPath, scriptContent);

			// Return the path to the newly created HTML file
			//await _dataRepository.InsertNewlyCreatedStream(postData.Name);
			
			return fullPath;
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
