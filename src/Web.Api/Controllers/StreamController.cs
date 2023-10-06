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
	}
}
