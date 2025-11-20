using GymHero.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymHero.Api.Endpoints;

public static class PlacesEndpoints
{
    public static void MapPlacesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/places")
            .WithTags("Places");

        // Search for nearby places (gyms, restaurants, etc.)
        group.MapGet("", async (
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] string type,
            [FromQuery] int radius,
            IGooglePlacesService placesService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Validate parameters
                if (lat < -90 || lat > 90)
                {
                    return Results.BadRequest(new { message = "Invalid latitude. Must be between -90 and 90." });
                }

                if (lng < -180 || lng > 180)
                {
                    return Results.BadRequest(new { message = "Invalid longitude. Must be between -180 and 180." });
                }

                if (radius < 1 || radius > 50000)
                {
                    return Results.BadRequest(new { message = "Invalid radius. Must be between 1 and 50000 meters." });
                }

                var response = await placesService.SearchNearbyAsync(lat, lng, type, radius);

                // Transform response to match frontend expectations
                var results = response.Results.Select(place => new
                {
                    place_id = place.PlaceId,
                    name = place.Name,
                    vicinity = place.Vicinity,
                    rating = place.Rating,
                    user_ratings_total = place.UserRatingsTotal,
                    geometry = new
                    {
                        location = new
                        {
                            lat = place.Location.Latitude,
                            lng = place.Location.Longitude
                        }
                    },
                    photos = place.Photos?.Select(photoRef => new
                    {
                        photo_reference = photoRef
                    }).ToList(),
                    opening_hours = place.OpenNow.HasValue ? new
                    {
                        open_now = place.OpenNow.Value
                    } : null
                }).ToList();

                return Results.Ok(new
                {
                    results = results,
                    status = response.Status,
                    next_page_token = response.NextPageToken
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    title: "Google Places API Error",
                    detail: ex.Message,
                    statusCode: 503
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Failed to search places",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("SearchNearbyPlaces")
        .WithSummary("Search for nearby places using Google Places API");

        // Geocode an address to coordinates
        group.MapGet("/geocode", async (
            [FromQuery] string address,
            IGooglePlacesService placesService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    return Results.BadRequest(new { message = "Address is required" });
                }

                var response = await placesService.GeocodeAddressAsync(address);

                return Results.Ok(new
                {
                    location = new
                    {
                        lat = response.Location.Latitude,
                        lng = response.Location.Longitude
                    },
                    formatted_address = response.FormattedAddress,
                    status = response.Status
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    title: "Geocoding Error",
                    detail: ex.Message,
                    statusCode: 503
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Failed to geocode address",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("GeocodeAddress")
        .WithSummary("Convert an address to geographic coordinates");

        // Get detailed information about a specific place
        group.MapGet("/details/{placeId}", async (
            string placeId,
            IGooglePlacesService placesService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placeId))
                {
                    return Results.BadRequest(new { message = "Place ID is required" });
                }

                var response = await placesService.GetPlaceDetailsAsync(placeId);

                return Results.Ok(new
                {
                    result = new
                    {
                        place_id = response.PlaceId,
                        name = response.Name,
                        formatted_address = response.FormattedAddress,
                        formatted_phone_number = response.FormattedPhoneNumber,
                        website = response.Website,
                        rating = response.Rating,
                        user_ratings_total = response.UserRatingsTotal,
                        geometry = new
                        {
                            location = new
                            {
                                lat = response.Location.Latitude,
                                lng = response.Location.Longitude
                            }
                        },
                        photos = response.Photos?.Select(photoRef => new
                        {
                            photo_reference = photoRef
                        }).ToList(),
                        opening_hours = response.OpeningHours != null ? new
                        {
                            open_now = response.OpeningHours.OpenNow,
                            weekday_text = response.OpeningHours.WeekdayText
                        } : null
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    title: "Place Details Error",
                    detail: ex.Message,
                    statusCode: 503
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Failed to get place details",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("GetPlaceDetails")
        .WithSummary("Get detailed information about a specific place");
    }
}
