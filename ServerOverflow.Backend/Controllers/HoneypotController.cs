using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MineProtocol;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using ServerOverflow.Backend.Models;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

namespace ServerOverflow.Backend.Controllers;

/// <summary>
/// Honeypot events
/// </summary>
[ApiController]
[Route("/api/honeypot")]
public class HoneypotController : ControllerBase {
    /// <summary>
    /// Retrieves an honeypot event entry
    /// </summary>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="404">Honeypot event with specified ID does not exist</response>
    /// <response code="200">Successfully retrieved honeypot event</response>
    /// <param name="id">Honeypot event ID</param>
    [HttpGet] [Route("{id}")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(HoneypotEvent), 200)]
    public async Task<IActionResult> Get(string id) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);

        var target = await HoneypotEvent.Get(id);
        if (target == null)
            return ValidationProblem(
                title: "Failed to retrieve honeypot event entry", 
                detail: "Honeypot event entry with specified ID does not exist",
                statusCode: 404);
        
        return Ok(target);
    }
    
    /// <summary>
    /// Retrieves an honeypot event entry
    /// </summary>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="403">User does not have required permission</response>
    /// <response code="200">Successfully reported honeypot event</response>
    [HttpPost] [Route("report")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(typeof(ValidationProblem), 404)]
    [ProducesResponseType(typeof(ValidationProblem), 403)]
    [ProducesResponseType(typeof(HoneypotEvent), 200)]
    public async Task<IActionResult> Report([FromBody] HoneypotEvent honeypotEvent) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);

        if (!account.HasPermission(Permission.HoneypotEvents))
            return ValidationProblem(
                title: "Required permission was not granted", 
                detail: "Reporting honeypot events required Honeypot Events permission",
                statusCode: 403);

        honeypotEvent.Id = ObjectId.GenerateNewId();
        honeypotEvent.Timestamp = DateTime.UtcNow;
        honeypotEvent.Description = honeypotEvent.ToString();
        await Database.HoneypotEvents.InsertOneAsync(honeypotEvent);

        try {
            var ass = Assembly.GetExecutingAssembly();
            await using var cityStream = ass.GetManifestResourceStream("GeoLite2-City.mmdb");
            await using var asnStream = ass.GetManifestResourceStream("GeoLite2-ASN.mmdb");
            using var cityDb = new DatabaseReader(cityStream!);
            using var asnDb = new DatabaseReader(asnStream!);

            var city = cityDb.City(honeypotEvent.SourceIp);
            var asn = asnDb.Asn(honeypotEvent.SourceIp);
            
            
            var webhook = new Webhook {
                Embeds = [
                    new Embed {
                        Title = honeypotEvent.Type switch {
                            HoneypotEventType.Joined =>
                                $"{honeypotEvent.Username} joined from {honeypotEvent.SourceIp}:{honeypotEvent.SourcePort}",
                            HoneypotEventType.Left =>
                                $"{honeypotEvent.Username} left from {honeypotEvent.SourceIp}:{honeypotEvent.SourcePort}",
                            _ => $"Server list ping from {honeypotEvent.SourceIp}:{honeypotEvent.SourcePort}"
                        },
                        Color = honeypotEvent.Type switch {
                            HoneypotEventType.Joined => Color.LimeGreen,
                            HoneypotEventType.Left => Color.IndianRed,
                            _ => Color.LightSeaGreen
                        },
                        Fields = [
                            new Field("ASN", asn.AutonomousSystemOrganization ?? $"AS{asn.AutonomousSystemNumber}", true),
                            new Field("City", city.City.Name ?? "Unknown", true),
                            new Field("Country", new RegionInfo(city.Country.IsoCode ?? "ZW").EnglishName, true),
                            new Field("Version", Resources.Protocol.TryGetValue(honeypotEvent.Protocol, out var version) ? version : "Unknown", true),
                            new Field("Operating system", honeypotEvent.OperatingSystem, true),
                            new Field("Protocol", honeypotEvent.Protocol.ToString(), true),
                            new Field("p0f signature", honeypotEvent.Signature)
                        ],
                        Footer = new Footer("95.141.241.193:25565"),
                        Timestamp = DateTime.Now
                    }
                ]
            };

            if (honeypotEvent.Uuid != null)
                webhook.Embeds[0].Fields!.Add(new Field("Player UUID", $"[{honeypotEvent.Uuid}](https://namemc.com/{honeypotEvent.Uuid})"));
        
            await webhook.Send();
        } catch (Exception e) {
            Log.Error("Failed to send webhook: {0}", e);
        }
        
        return Ok(honeypotEvent);
    }

    /// <summary>
    /// Searches for honeypot event entries
    /// </summary>
    /// <response code="401">Invalid API key or cookie</response>
    /// <response code="204">Zero results for specified query</response>
    /// <response code="200">Successfully retrieved honeypot event entries</response>
    /// <param name="query">Query with operators</param>
    /// <param name="page">Page number from 1</param>
    /// <param name="startId">Honeypot event ID to start pagination from</param>
    [HttpPost] [Route("search")]
    [ProducesResponseType(typeof(ValidationProblem), 401)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(PaginationModel<HoneypotEvent>), 200)]
    public async Task<IActionResult> Search([FromQuery] string query = "", [FromQuery] int page = 1, [FromQuery] string? startId = null) {
        var account = await HttpContext.GetAccount();
        if (account == null)
            return ValidationProblem(
                title: "Invalid API key or cookie",
                detail: "Failed to retrieve API key or account",
                statusCode: 401);
        
        var model = new PaginationModel<HoneypotEvent> {
            CurrentPage = page, Query = query
        };

        if (model.CurrentPage < 1)
            model.CurrentPage = 1;

        try {
            var watch = new Stopwatch(); watch.Start();
            var doc = Query.HoneypotEvent(model.Query!);
            if (startId != null)
                doc &= Builders<HoneypotEvent>.Filter.Lte(x => x.Id, new ObjectId(startId));
            var sort = Builders<HoneypotEvent>.Sort.Descending(x => x.Id); 
            var find = Database.HoneypotEvents.Find(doc).Sort(sort);
            model.TotalMatches = await find.CountAsync();
            if (model.TotalMatches == 0) return NoContent();
            
            model.TotalPages = (int)Math.Ceiling(model.TotalMatches / 50f);
            if (model.CurrentPage > model.TotalPages)
                model.CurrentPage = model.TotalPages;
        
            using var cursor = await find.Skip(50 * (model.CurrentPage-1)).Limit(50).ToCursorAsync();
            model.Items = []; model.Items.AddRange(cursor.ToList());
            watch.Stop(); model.Milliseconds = watch.ElapsedMilliseconds;
            return Ok(model);
        } catch (Exception e) {
            return ValidationProblem(
                title: e.GetType().Name, 
                detail: e.Message);
        }
    }
}