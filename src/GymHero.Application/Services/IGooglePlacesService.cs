namespace GymHero.Application.Services;

/// <summary>
/// Service for interacting with Google Places API
/// </summary>
public interface IGooglePlacesService
{
    /// <summary>
    /// Searches for nearby places based on location and type
    /// </summary>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <param name="type">Place type (e.g., "gym", "restaurant")</param>
    /// <param name="radius">Search radius in meters (default: 5000)</param>
    /// <returns>List of nearby places</returns>
    Task<NearbyPlacesResponse> SearchNearbyAsync(
        double latitude,
        double longitude,
        string type = "gym",
        int radius = 5000);

    /// <summary>
    /// Converts an address to geographic coordinates
    /// </summary>
    /// <param name="address">Street address or location description</param>
    /// <returns>Geographic coordinates</returns>
    Task<GeocodingResponse> GeocodeAddressAsync(string address);

    /// <summary>
    /// Gets detailed information about a specific place
    /// </summary>
    /// <param name="placeId">Google Place ID</param>
    /// <returns>Place details</returns>
    Task<PlaceDetailsResponse> GetPlaceDetailsAsync(string placeId);
}

/// <summary>
/// Response containing nearby places
/// </summary>
public record NearbyPlacesResponse(
    List<PlaceResult> Results,
    string Status,
    string? NextPageToken = null
);

/// <summary>
/// Individual place result
/// </summary>
public record PlaceResult(
    string PlaceId,
    string Name,
    string? Vicinity,
    double? Rating,
    int? UserRatingsTotal,
    Location Location,
    List<string>? Photos,
    bool? OpenNow,
    string? PriceLevel,
    List<string>? Types
);

/// <summary>
/// Geographic location coordinates
/// </summary>
public record Location(
    double Latitude,
    double Longitude
);

/// <summary>
/// Response from geocoding an address
/// </summary>
public record GeocodingResponse(
    Location Location,
    string FormattedAddress,
    string Status
);

/// <summary>
/// Detailed place information
/// </summary>
public record PlaceDetailsResponse(
    string PlaceId,
    string Name,
    string? FormattedAddress,
    string? FormattedPhoneNumber,
    string? Website,
    double? Rating,
    int? UserRatingsTotal,
    Location Location,
    List<string>? Photos,
    OpeningHours? OpeningHours
);

/// <summary>
/// Opening hours information
/// </summary>
public record OpeningHours(
    bool OpenNow,
    List<string>? WeekdayText
);
