using System.Text.Json;
using GymHero.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GymHero.Infrastructure.Services;

/// <summary>
/// Service for interacting with Google Places API using direct HTTP calls
/// </summary>
public class GooglePlacesService : IGooglePlacesService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GooglePlacesService> _logger;
    private const string PlacesBaseUrl = "https://maps.googleapis.com/maps/api/place";
    private const string GeocodingBaseUrl = "https://maps.googleapis.com/maps/api/geocode";

    public GooglePlacesService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GooglePlacesService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _apiKey = configuration["GooglePlaces:ApiKey"]
            ?? throw new InvalidOperationException("Google Places API key is not configured");
    }

    public async Task<NearbyPlacesResponse> SearchNearbyAsync(
        double latitude,
        double longitude,
        string type = "gym",
        int radius = 5000)
    {
        try
        {
            var url = $"{PlacesBaseUrl}/nearbysearch/json?location={latitude},{longitude}&radius={radius}&type={type}&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<GoogleNearbySearchResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data == null || data.Status != "OK" && data.Status != "ZERO_RESULTS")
            {
                _logger.LogError("Google Places API error: {Status}", data?.Status);
                throw new InvalidOperationException($"Google Places API error: {data?.Status}");
            }

            var results = data.Results?.Select(place => new PlaceResult(
                PlaceId: place.PlaceId ?? string.Empty,
                Name: place.Name ?? string.Empty,
                Vicinity: place.Vicinity,
                Rating: place.Rating,
                UserRatingsTotal: place.UserRatingsTotal,
                Location: new Application.Services.Location(
                    place.Geometry?.Location?.Lat ?? 0,
                    place.Geometry?.Location?.Lng ?? 0
                ),
                Photos: place.Photos?.Select(p => p.PhotoReference ?? string.Empty).Where(p => !string.IsNullOrEmpty(p)).ToList(),
                OpenNow: place.OpeningHours?.OpenNow,
                PriceLevel: place.PriceLevel?.ToString(),
                Types: place.Types
            )).ToList() ?? new List<PlaceResult>();

            _logger.LogInformation("Found {Count} places near ({Lat}, {Lng})",
                results.Count, latitude, longitude);

            return new NearbyPlacesResponse(
                Results: results,
                Status: data.Status,
                NextPageToken: data.NextPageToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search nearby places");
            throw;
        }
    }

    public async Task<GeocodingResponse> GeocodeAddressAsync(string address)
    {
        try
        {
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"{GeocodingBaseUrl}/json?address={encodedAddress}&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<GoogleGeocodingResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data == null || data.Status != "OK")
            {
                _logger.LogError("Geocoding API error: {Status}", data?.Status);
                throw new InvalidOperationException($"Geocoding API error: {data?.Status}");
            }

            var result = data.Results?.FirstOrDefault()
                ?? throw new InvalidOperationException("No results found for address");

            _logger.LogInformation("Geocoded address: {Address} -> ({Lat}, {Lng})",
                address, result.Geometry?.Location?.Lat, result.Geometry?.Location?.Lng);

            return new GeocodingResponse(
                Location: new Application.Services.Location(
                    result.Geometry?.Location?.Lat ?? 0,
                    result.Geometry?.Location?.Lng ?? 0
                ),
                FormattedAddress: result.FormattedAddress ?? string.Empty,
                Status: data.Status
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to geocode address: {Address}", address);
            throw;
        }
    }

    public async Task<PlaceDetailsResponse> GetPlaceDetailsAsync(string placeId)
    {
        try
        {
            var url = $"{PlacesBaseUrl}/details/json?place_id={placeId}&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<GooglePlaceDetailsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data == null || data.Status != "OK")
            {
                _logger.LogError("Place Details API error: {Status}", data?.Status);
                throw new InvalidOperationException($"Place Details API error: {data?.Status}");
            }

            var result = data.Result
                ?? throw new InvalidOperationException("No result found for place");

            _logger.LogInformation("Retrieved details for place: {PlaceId} - {Name}",
                placeId, result.Name);

            return new PlaceDetailsResponse(
                PlaceId: result.PlaceId ?? string.Empty,
                Name: result.Name ?? string.Empty,
                FormattedAddress: result.FormattedAddress,
                FormattedPhoneNumber: result.FormattedPhoneNumber,
                Website: result.Website,
                Rating: result.Rating,
                UserRatingsTotal: result.UserRatingsTotal,
                Location: new Application.Services.Location(
                    result.Geometry?.Location?.Lat ?? 0,
                    result.Geometry?.Location?.Lng ?? 0
                ),
                Photos: result.Photos?.Select(p => p.PhotoReference ?? string.Empty).Where(p => !string.IsNullOrEmpty(p)).ToList(),
                OpeningHours: result.OpeningHours != null
                    ? new Application.Services.OpeningHours(
                        result.OpeningHours.OpenNow,
                        result.OpeningHours.WeekdayText
                    )
                    : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get place details: {PlaceId}", placeId);
            throw;
        }
    }

    // Google API JSON response models
    private class GoogleNearbySearchResponse
    {
        public List<GooglePlace>? Results { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? NextPageToken { get; set; }
    }

    private class GoogleGeocodingResponse
    {
        public List<GoogleGeocodingResult>? Results { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private class GooglePlaceDetailsResponse
    {
        public GooglePlaceDetails? Result { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private class GooglePlace
    {
        public string? PlaceId { get; set; }
        public string? Name { get; set; }
        public string? Vicinity { get; set; }
        public double? Rating { get; set; }
        public int? UserRatingsTotal { get; set; }
        public GoogleGeometry? Geometry { get; set; }
        public List<GooglePhoto>? Photos { get; set; }
        public GoogleOpeningHours? OpeningHours { get; set; }
        public int? PriceLevel { get; set; }
        public List<string>? Types { get; set; }
    }

    private class GooglePlaceDetails
    {
        public string? PlaceId { get; set; }
        public string? Name { get; set; }
        public string? FormattedAddress { get; set; }
        public string? FormattedPhoneNumber { get; set; }
        public string? Website { get; set; }
        public double? Rating { get; set; }
        public int? UserRatingsTotal { get; set; }
        public GoogleGeometry? Geometry { get; set; }
        public List<GooglePhoto>? Photos { get; set; }
        public GoogleOpeningHoursDetails? OpeningHours { get; set; }
    }

    private class GoogleGeocodingResult
    {
        public string? FormattedAddress { get; set; }
        public GoogleGeometry? Geometry { get; set; }
    }

    private class GoogleGeometry
    {
        public GoogleLocation? Location { get; set; }
    }

    private class GoogleLocation
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    private class GooglePhoto
    {
        public string? PhotoReference { get; set; }
    }

    private class GoogleOpeningHours
    {
        public bool OpenNow { get; set; }
    }

    private class GoogleOpeningHoursDetails
    {
        public bool OpenNow { get; set; }
        public List<string>? WeekdayText { get; set; }
    }
}
