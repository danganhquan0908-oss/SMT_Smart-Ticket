using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

var baseUrls = (Environment.GetEnvironmentVariable("SMARTTICKET_LOAD_BASE_URLS")
    ?? "http://localhost:5186,http://localhost:5187")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
var tripId = Environment.GetEnvironmentVariable("SMARTTICKET_LOAD_TRIP_ID")
    ?? "51173efe-da1b-4315-a437-6c4957ee652a";
var seatCode = Environment.GetEnvironmentVariable("SMARTTICKET_LOAD_SEAT_CODE") ?? "A01";
var searchPath = Environment.GetEnvironmentVariable("SMARTTICKET_LOAD_SEARCH_PATH")
    ?? "/api/search/trips?departureStationId=257393a5-4bb5-488d-be96-e38543ad387a&arrivalStationId=3c90d56a-515c-4a5f-817c-163da6c9a413&travelDate=2026-11-07";
var requestsPerClient = int.TryParse(Environment.GetEnvironmentVariable("SMARTTICKET_LOAD_REQUESTS_PER_CLIENT"), out var parsedRequests)
    ? Math.Clamp(parsedRequests, 1, 20)
    : 4;
var seatContentionClients = int.TryParse(Environment.GetEnvironmentVariable("SMARTTICKET_LOAD_SEAT_CONTENTION_CLIENTS"), out var parsedContentionClients)
    ? Math.Clamp(parsedContentionClients, 1, 10000)
    : 128;

var handler = new SocketsHttpHandler
{
    MaxConnectionsPerServer = 10000,
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    UseProxy = false
};
using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

foreach (var baseUrl in baseUrls)
{
    using var response = await client.GetAsync($"{baseUrl}/api/catalog/stations");
    if (!response.IsSuccessStatusCode)
        throw new InvalidOperationException($"App at {baseUrl} is unhealthy: HTTP {(int)response.StatusCode}.");
}

Console.WriteLine($"Targets: {string.Join(", ", baseUrls)}");
Console.WriteLine($"Trip: {tripId}; contested seat: {seatCode}; requests/client: {requestsPerClient}");

var readConcurrencyLevels = (Environment.GetEnvironmentVariable("SMARTTICKET_LOAD_CONCURRENCY")
    ?? "25,100,250,500")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(value => int.TryParse(value, out var concurrency) ? concurrency : 0)
    .Where(concurrency => concurrency is > 0 and <= 200000)
    .ToArray();
if (readConcurrencyLevels.Length == 0)
    throw new InvalidOperationException("SMARTTICKET_LOAD_CONCURRENCY must contain at least one concurrency level from 1 to 200000.");
var readResults = new List<LoadResult>();
foreach (var concurrency in readConcurrencyLevels)
{
    var result = await RunReadLoadAsync(concurrency, requestsPerClient);
    readResults.Add(result);
    Print(result);
}

var seatResult = await RunSeatContentionAsync(client, baseUrls, tripId, seatCode, concurrency: seatContentionClients);
Print(seatResult);

var failedReadRuns = readResults.Count(x => x.Failed > 0);
var successCount = seatResult.StatusCounts.GetValueOrDefault(201);
var conflictCount = seatResult.StatusCounts.GetValueOrDefault(409);
var contentionPassed = successCount == 1 && conflictCount == seatResult.Requests - 1;
Console.WriteLine($"Read load runs with HTTP/network failures: {failedReadRuns}/{readResults.Count}");
Console.WriteLine($"Seat contention invariant: {(contentionPassed ? "PASS" : "FAIL")} (201={successCount}, 409={conflictCount})");

return failedReadRuns == 0 && contentionPassed ? 0 : 1;

async Task<LoadResult> RunReadLoadAsync(int concurrency, int requestsPerClient)
{
    var requestCount = concurrency * requestsPerClient;
    var latencies = new ConcurrentBag<double>();
    var statusCounts = new ConcurrentDictionary<int, int>();
    var failures = 0;
    var started = Stopwatch.StartNew();

    await Parallel.ForEachAsync(Enumerable.Range(0, requestCount),
        new ParallelOptions { MaxDegreeOfParallelism = concurrency },
        async (index, cancellationToken) =>
        {
            var target = baseUrls[index % baseUrls.Length];
            var uri = (index % 3) switch
            {
                0 => $"{target}/api/trips/{tripId}/seats",
                1 => $"{target}/api/catalog/stations",
                _ => $"{target}{searchPath}"
            };
            var timer = Stopwatch.StartNew();
            try
            {
                using var response = await client.GetAsync(uri, cancellationToken);
                timer.Stop();
                latencies.Add(timer.Elapsed.TotalMilliseconds);
                statusCounts.AddOrUpdate((int)response.StatusCode, 1, (_, count) => count + 1);
                if (!response.IsSuccessStatusCode) Interlocked.Increment(ref failures);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                timer.Stop();
                latencies.Add(timer.Elapsed.TotalMilliseconds);
                statusCounts.AddOrUpdate(0, 1, (_, count) => count + 1);
                Interlocked.Increment(ref failures);
            }
        });

    started.Stop();
    return new LoadResult($"Read concurrency {concurrency}", requestCount, started.Elapsed, latencies.ToArray(), new Dictionary<int, int>(statusCounts), failures);
}

async Task<LoadResult> RunSeatContentionAsync(HttpClient httpClient, string[] targets, string tripId, string seatCode, int concurrency)
{
    var latencies = new ConcurrentBag<double>();
    var statusCounts = new ConcurrentDictionary<int, int>();
    var failures = 0;
    var started = Stopwatch.StartNew();

    await Parallel.ForEachAsync(Enumerable.Range(0, concurrency),
        new ParallelOptions { MaxDegreeOfParallelism = concurrency },
        async (index, cancellationToken) =>
        {
            var target = targets[index % targets.Length];
            var timer = Stopwatch.StartNew();
            try
            {
                using var response = await httpClient.PostAsJsonAsync(
                    $"{target}/api/trips/{tripId}/holds",
                    new
                    {
                        passengerName = $"Load Test {index:D3}",
                        passengerPhone = $"099{index:D7}",
                        passengerEmail = $"load-{index}@example.test",
                        seatCodes = new[] { seatCode }
                    },
                    cancellationToken);
                timer.Stop();
                latencies.Add(timer.Elapsed.TotalMilliseconds);
                statusCounts.AddOrUpdate((int)response.StatusCode, 1, (_, count) => count + 1);
                if (response.StatusCode is not (HttpStatusCode.Created or HttpStatusCode.Conflict))
                    Interlocked.Increment(ref failures);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                timer.Stop();
                latencies.Add(timer.Elapsed.TotalMilliseconds);
                statusCounts.AddOrUpdate(0, 1, (_, count) => count + 1);
                Interlocked.Increment(ref failures);
            }
        });

    started.Stop();
    return new LoadResult("Seat contention (two app instances)", concurrency, started.Elapsed, latencies.ToArray(), new Dictionary<int, int>(statusCounts), failures);
}

void Print(LoadResult result)
{
    var sorted = result.LatenciesMs.Order().ToArray();
    var statuses = string.Join(", ", result.StatusCounts.OrderBy(x => x.Key).Select(x => $"{(x.Key == 0 ? "network" : x.Key.ToString())}={x.Value}"));
    Console.WriteLine($"{result.Name}: requests={result.Requests}, elapsed={result.Duration.TotalSeconds:F2}s, throughput={result.Requests / Math.Max(result.Duration.TotalSeconds, 0.001):F1}/s, p50={Percentile(sorted, .50):F0}ms, p95={Percentile(sorted, .95):F0}ms, p99={Percentile(sorted, .99):F0}ms, max={sorted.LastOrDefault():F0}ms, failures={result.Failed}, statuses=[{statuses}]");
}

static double Percentile(double[] sortedValues, double percentile)
{
    if (sortedValues.Length == 0) return 0;
    var index = (int)Math.Ceiling(percentile * sortedValues.Length) - 1;
    return sortedValues[Math.Clamp(index, 0, sortedValues.Length - 1)];
}

sealed record LoadResult(string Name, int Requests, TimeSpan Duration, double[] LatenciesMs, Dictionary<int, int> StatusCounts, int Failed);
