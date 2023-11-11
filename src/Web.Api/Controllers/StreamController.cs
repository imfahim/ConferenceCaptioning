using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;
using System.Text.Json;
using Web.Api.Model.Request;
using Web.Api.Model.Response;
using Web.Api.Service;

namespace Web.Api.Controllers
{
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class StreamController : ControllerBase
	{
		private readonly IStreamService _streamService;
		private string connectionString = "YourConnectionString"; // Replace with your actual connection string
		//private DataRepository repository = new DataRepository(connectionString);

		public StreamController(IStreamService streamService)
		{
			_streamService = streamService;
		}

		[HttpPost("create-stream")]
		[ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> CreateStreamWithStyle([FromBody] RequestModel postData)
		{
			try
			{
				var path = await _streamService.CreateStream(postData);
				//repository.InsertData("John Doe", 30); // Example data

				return Ok(new ResponseModel() { FilePath = path });
			}
			catch (DuplicateNameException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
			}
		}

		[HttpPost("view-counter")]
		[ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> UserViewed([FromBody] string streamName)
		{
			try
			{
				await _streamService.AddViewerOnStream(streamName);
				return Ok();
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
			}
		}

		//[HttpGet]
		//[ProducesResponseType(typeof(ResponseModel), (int)HttpStatusCode.OK)]
		//public async Task<IActionResult> GetAllCreatedStream()
		//{
		//	try
		//	{
		//		var path = await _streamService.CreateStream(postData);
		//		//repository.InsertData("John Doe", 30); // Example data

		//		return Ok(new ResponseModel() { FilePath = path });
		//	}
		//	catch (DuplicateNameException ex)
		//	{
		//		return BadRequest(ex.Message);
		//	}
		//	catch (Exception ex)
		//	{
		//		return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
		//	}
		//}
	}
}
