using Web.Api.Service;

namespace Web.Api.BackgroundJobs;

public class AnalyticJob : CronJobService
{
	private readonly ILogger<AnalyticJob> _logger;
	private readonly IServiceProvider _serviceProvider;

	public AnalyticJob(IScheduleConfig<AnalyticJob> config, ILogger<AnalyticJob> logger, IServiceProvider serviceProvider)
		: base(config.CronExpression, config.TimeZoneInfo)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
	}

	public override Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("AnalyticJob starts.");
		return base.StartAsync(cancellationToken);
	}

	public override async Task Execute(CancellationToken cancellationToken)
	{
		_logger.LogInformation("{now} AnalyticJob is working.", DateTime.Now.ToString("T"));
		try
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var streamService = scope.ServiceProvider.GetRequiredService<IStreamService>();
				await streamService.SendAnalytics();
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"{DateTime.Now.ToString("T")} AnalyticJob Error. Message: {ex.Message}");
			_logger.LogTrace(ex.StackTrace);
		}
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("AnalyticJob is stopping.");
		return base.StopAsync(cancellationToken);
	}
}

