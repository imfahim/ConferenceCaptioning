using System.Data;
using Web.Api.Model;
using Web.Api.Model.Request;
using Web.Api.Repository;

namespace Web.Api.Service
{
	public class StreamService : IStreamService
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IDataRepository _dataRepository;
		public StreamService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment, IDataRepository dataRepository)
		{
			_httpContextAccessor = httpContextAccessor;
			_webHostEnvironment = webHostEnvironment;
			_dataRepository = dataRepository;
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
			var domain = $"{protocol}://{host}{pathBase}";
			//var domain = $"https://10.62.40.174:45455";

			var fullPath = $"{domain}/{postData.Name}/index.html";

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
			scriptContent = scriptContent.Replace("serverDomain", domain);

			CopyFilesRecursively(Path.Combine(_webHostEnvironment.ContentRootPath, "template"),
				organizerFolderPath);

			// Save the modified HTML content to the new file
			await File.WriteAllTextAsync(filePath, htmlContent);

			// Save the modified script.js to the organizer folder
			string scriptPath = Path.Combine(organizerFolderPath, "script.js");
			await File.WriteAllTextAsync(scriptPath, scriptContent);

			// Return the path to the newly created HTML file
			await _dataRepository.InsertNewlyCreatedStream(postData.Name);
			
			return fullPath;
		}

		public async Task AddViewerOnStream(string streamName)
		{
			await _dataRepository.AddViewerOnStream(streamName);
		}

		public async Task SendAnalytics()
		{
			var createdStream = await _dataRepository.GetLastDayCreatedStream();
			var viewdStream = await _dataRepository.GetLastDayViewedStream();
			var emailBody = CreateAnalyticsEmailBody(createdStream, viewdStream);
			var emailService = new EmailService();
			await emailService.SendEmailAsync("fahim@brainstation-23.com", $"Analytics On - {DateTime.Now.ToString("yyyy-MM-dd")}", emailBody);

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

		public string CreateAnalyticsEmailBody(List<StreamCreationTracker> creationTrackers, List<StreamViewerTracker> viewerTrackers)
		{
			string emailBody = @"
        <html>
        <head>
            <style>
                table {
                    border-collapse: collapse;
                    width: 100%;
                }
                th, td {
                    border: 1px solid #ccc;
                    padding: 8px;
                    text-align: left;
                }
                th {
                    background-color: #f2f2f2;
                }
            </style>
        </head>
        <body>
            <h1>Analytics Report</h1>
            <h2>Stream Creation</h2>
            <table>
                <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>Created On</th>
                </tr>";

			foreach (var creationTracker in creationTrackers)
			{
				emailBody += $@"
            <tr>
                <td>{creationTracker.Id}</td>
                <td>{creationTracker.Name}</td>
                <td>{creationTracker.CreatedOn}</td>
            </tr>";
			}

			emailBody += @"
            </table>
            <h2>Stream Viewer</h2>
            <table>
                <tr>
                    <th>ID</th>
                    <th>View Count</th>
                    <th>Name</th>
                    <th>View Date</th>
                </tr>";

			foreach (var viewerTracker in viewerTrackers)
			{
				emailBody += $@"
            <tr>
                <td>{viewerTracker.Id}</td>
                <td>{viewerTracker.ViewCount}</td>
                <td>{viewerTracker.Name}</td>
                <td>{viewerTracker.ViewDate}</td>
            </tr>";
			}

			emailBody += @"
            </table>
        </body>
        </html>";

			return emailBody;
		}

	}
}
