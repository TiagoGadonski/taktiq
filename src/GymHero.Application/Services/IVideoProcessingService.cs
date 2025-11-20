namespace GymHero.Application.Services;

/// <summary>
/// Service for processing video files
/// </summary>
public interface IVideoProcessingService
{
    /// <summary>
    /// Generates a thumbnail from a video file
    /// </summary>
    /// <param name="videoStream">Video file stream</param>
    /// <param name="timeSpan">Time position to capture thumbnail (default: 1 second)</param>
    /// <returns>Thumbnail image as byte array</returns>
    Task<byte[]> GenerateThumbnailAsync(Stream videoStream, TimeSpan? timeSpan = null);

    /// <summary>
    /// Extracts video metadata (duration, dimensions)
    /// </summary>
    /// <param name="videoStream">Video file stream</param>
    /// <returns>Video metadata</returns>
    Task<VideoMetadata> GetVideoMetadataAsync(Stream videoStream);

    /// <summary>
    /// Compresses and optimizes a video file
    /// </summary>
    /// <param name="videoStream">Original video file stream</param>
    /// <param name="quality">Compression quality preset</param>
    /// <param name="maxResolution">Maximum resolution (width)</param>
    /// <returns>Compressed video as byte array and metadata</returns>
    Task<(byte[] videoData, VideoMetadata metadata)> CompressVideoAsync(
        Stream videoStream,
        VideoQuality quality = VideoQuality.Medium,
        int? maxResolution = null);
}

/// <summary>
/// Video metadata information
/// </summary>
public record VideoMetadata(
    double DurationSeconds,
    int Width,
    int Height,
    string Format
);

/// <summary>
/// Video compression quality presets
/// </summary>
public enum VideoQuality
{
    /// <summary>
    /// Low quality, high compression (smaller file size)
    /// Target: ~500 kbps video bitrate
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium quality, balanced compression
    /// Target: ~1500 kbps video bitrate
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High quality, low compression (larger file size)
    /// Target: ~3000 kbps video bitrate
    /// </summary>
    High = 2
}
