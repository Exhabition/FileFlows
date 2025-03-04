﻿using Mysqlx.Datatypes;
using Microsoft.AspNetCore.Mvc;
using FileFlows.Server.Helpers;
using FileFlows.Server.Services;

namespace FileFlows.Server.Controllers;


/// <summary>
/// Status controller
/// </summary>
[Route("/api/statistics")]
public class StatisticsController : Controller
{
    /// <summary>
    /// Records a statistic
    /// </summary>
    /// <param name="statistic">the statistic to record</param>
    [HttpPost("record")]
    public Task Record([FromBody] Statistic statistic)
        => new StatisticService().Record(statistic);

    /// <summary>
    /// Gets statistics by name
    /// </summary>
    /// <returns>the matching statistics</returns>
    [HttpGet("by-name/{name}")]
    public Task<IEnumerable<Statistic>> GetStatisticsByName([FromRoute] string name)
        => new StatisticService().GetStatisticsByName(name);

    /// <summary>
    /// Clears statistics for
    /// </summary>
    /// <param name="name">[Optional] the name of the statistic to clear</param>
    /// <param name="before">[Optional] the date of the statistic to clear</param>
    /// <returns>the response</returns>
    [HttpPost("clear")]
    public IActionResult Clear([FromQuery] string? name = null, DateTime? before = null)
    {
        try
        {
            new StatisticService().Clear(name, before);
            return Ok();
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }
}


/// <summary>
/// A statistic
/// </summary>
public class Statistic
{
    /// <summary>
    /// Gets or sets the name of the statistic
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value
    /// </summary>
    public object Value { get; set; }
}