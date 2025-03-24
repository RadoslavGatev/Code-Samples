using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace CronEvaluation;

public class EvaluateCronTrigger
{
    private readonly ILogger<EvaluateCronTrigger> _logger;

    public EvaluateCronTrigger(ILogger<EvaluateCronTrigger> logger)
    {
        _logger = logger;
    }

    [Function("EvaluateCron")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        var cronExpression = req.Query["cron"];
        var currentTimeString = req.Query["currentTime"];

        if (string.IsNullOrWhiteSpace(cronExpression) || string.IsNullOrWhiteSpace(currentTimeString))
        {
            return new BadRequestObjectResult("Please provide both cron expression and current time.");
        }

        if (!DateTime.TryParse(currentTimeString, null, DateTimeStyles.AdjustToUniversal, out DateTime currentTime))
        {
            return new BadRequestObjectResult("Invalid current time format.");
        }

        // Adjust the current time to the start of the minute
        var currentTimeAtZeroSeconds = currentTime.AddSeconds(-currentTime.Second);
        var previousMinute = currentTimeAtZeroSeconds.AddSeconds(-1);

        var schedule = CrontabSchedule.Parse(cronExpression);
        var nextOccurrence = schedule.GetNextOccurrence(previousMinute);

        bool isMatch = nextOccurrence == currentTimeAtZeroSeconds;

        return new OkObjectResult(isMatch);
    }
}
