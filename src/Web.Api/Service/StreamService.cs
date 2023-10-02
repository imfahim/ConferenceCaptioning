using System.Data;

namespace Web.Api.Service
{
	public class StreamService : IStreamService
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		public StreamService(IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
		}
		public async Task<string> CreateStream(string streamName, string? bannerColor)
		{
			// Ensure the Organizer folder exists
			string organizerFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Organizer");
			Directory.CreateDirectory(organizerFolderPath);

			// Check if the HTML file already exists
			string filePath = Path.Combine(organizerFolderPath, $"{streamName}.html");
			if (File.Exists(filePath))
			{
				throw new DuplicateNameException($"HTML file with name '{streamName}' already exists.");
			}

			// Read the content of the template HTML file
			string templateFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "template", "index.html");
			string htmlContent = await File.ReadAllTextAsync(templateFilePath);

			// Replace placeholders in the template with the provided name
			htmlContent = htmlContent.Replace("{{NamePlaceholder}}", streamName);

			if (!string.IsNullOrEmpty(bannerColor))
			{
				htmlContent = htmlContent.Replace("{{BannerColor}}", bannerColor);
			}

			// Save the modified HTML content to the new file
			await File.WriteAllTextAsync(filePath, htmlContent);

			// Return the path to the newly created HTML file
			return filePath;
		}
	}
}
