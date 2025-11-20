using FFMpegCore;
using FFMpegCore.Enums;
using GymHero.Application.Services;
using Microsoft.Extensions.Logging;

namespace GymHero.Infrastructure.Services;

/// <summary>
/// Service for processing video files using FFMpeg
/// </summary>
public class VideoProcessingService : IVideoProcessingService
{
    private readonly ILogger<VideoProcessingService> _logger;

    public VideoProcessingService(ILogger<VideoProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<(byte[] videoData, VideoMetadata metadata)> CompressVideoAsync(
        Stream videoStream,
        VideoQuality quality = VideoQuality.Medium,
        int? maxResolution = null)
    {
        try
        {
            // Save stream to temporary file (FFMpeg requires file paths)
            var tempInputPath = Path.GetTempFileName();
            var tempOutputPath = Path.ChangeExtension(Path.GetTempFileName(), ".mp4");

            try
            {
                // Write input stream to temp file
                using (var fileStream = File.Create(tempInputPath))
                {
                    await videoStream.CopyToAsync(fileStream);
                }

                // Get original video metadata
                var mediaInfo = await FFProbe.AnalyseAsync(tempInputPath);
                var videoStreamInfo = mediaInfo.VideoStreams.FirstOrDefault()
                    ?? throw new InvalidOperationException("No video stream found");

                // Determine target bitrates based on quality preset
                var (videoBitrate, audioBitrate) = quality switch
                {
                    VideoQuality.Low => (500, 64),      // 500 kbps video, 64 kbps audio
                    VideoQuality.Medium => (1500, 128), // 1.5 Mbps video, 128 kbps audio
                    VideoQuality.High => (3000, 192),   // 3 Mbps video, 192 kbps audio
                    _ => (1500, 128)
                };

                // Determine target resolution
                int targetWidth = videoStreamInfo.Width;
                int targetHeight = videoStreamInfo.Height;

                if (maxResolution.HasValue && targetWidth > maxResolution.Value)
                {
                    // Scale down maintaining aspect ratio
                    var aspectRatio = (double)targetHeight / targetWidth;
                    targetWidth = maxResolution.Value;
                    targetHeight = (int)(maxResolution.Value * aspectRatio);

                    // Ensure even dimensions (required for H.264)
                    if (targetHeight % 2 != 0) targetHeight--;
                }

                _logger.LogInformation(
                    "Compressing video: {OriginalWidth}x{OriginalHeight} -> {TargetWidth}x{TargetHeight}, Quality: {Quality}, Bitrate: {VideoBitrate}k",
                    videoStreamInfo.Width, videoStreamInfo.Height, targetWidth, targetHeight, quality, videoBitrate);

                // Compress video using FFMpeg
                await FFMpegArguments
                    .FromFileInput(tempInputPath)
                    .OutputToFile(tempOutputPath, overwrite: true, options => options
                        .WithVideoCodec(VideoCodec.LibX264)        // H.264 codec for compatibility
                        .WithVideoBitrate(videoBitrate)            // Target video bitrate
                        .WithAudioCodec(AudioCodec.Aac)            // AAC audio codec
                        .WithAudioBitrate(audioBitrate)            // Target audio bitrate
                        .WithVideoFilters(filterOptions => filterOptions
                            .Scale(targetWidth, targetHeight))     // Resize if needed
                        .WithConstantRateFactor(quality switch     // CRF for quality control
                        {
                            VideoQuality.Low => 28,                // Higher CRF = more compression
                            VideoQuality.Medium => 23,             // Balanced
                            VideoQuality.High => 18,               // Lower CRF = better quality
                            _ => 23
                        })
                        .WithSpeedPreset(Speed.Medium)             // Encoding speed vs compression
                        .WithFastStart())                          // Enable streaming (moov atom at start)
                    .ProcessAsynchronously();

                // Read compressed video
                var compressedVideoData = await File.ReadAllBytesAsync(tempOutputPath);

                // Get metadata of compressed video
                var compressedMediaInfo = await FFProbe.AnalyseAsync(tempOutputPath);
                var compressedVideoStream = compressedMediaInfo.VideoStreams.FirstOrDefault()
                    ?? throw new InvalidOperationException("Failed to analyze compressed video");

                var compressedMetadata = new VideoMetadata(
                    DurationSeconds: compressedMediaInfo.Duration.TotalSeconds,
                    Width: compressedVideoStream.Width,
                    Height: compressedVideoStream.Height,
                    Format: compressedMediaInfo.Format.FormatName
                );

                var originalSize = new FileInfo(tempInputPath).Length;
                var compressedSize = compressedVideoData.Length;
                var compressionRatio = (1 - (double)compressedSize / originalSize) * 100;

                _logger.LogInformation(
                    "Video compression complete: {OriginalSize}MB -> {CompressedSize}MB ({CompressionRatio:F1}% reduction)",
                    originalSize / 1024.0 / 1024.0,
                    compressedSize / 1024.0 / 1024.0,
                    compressionRatio);

                return (compressedVideoData, compressedMetadata);
            }
            finally
            {
                // Cleanup temp files
                if (File.Exists(tempInputPath))
                {
                    File.Delete(tempInputPath);
                }
                if (File.Exists(tempOutputPath))
                {
                    File.Delete(tempOutputPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compress video");
            throw new InvalidOperationException("Failed to compress video", ex);
        }
    }

    public async Task<byte[]> GenerateThumbnailAsync(Stream videoStream, TimeSpan? timeSpan = null)
    {
        try
        {
            var captureTime = timeSpan ?? TimeSpan.FromSeconds(1);

            // Save stream to temporary file (FFMpeg requires file path)
            var tempVideoPath = Path.GetTempFileName();
            var tempThumbnailPath = Path.ChangeExtension(Path.GetTempFileName(), ".jpg");

            try
            {
                // Write stream to temp file
                using (var fileStream = File.Create(tempVideoPath))
                {
                    await videoStream.CopyToAsync(fileStream);
                }

                // Generate thumbnail using FFMpeg (720p thumbnail)
                var snapshot = await FFMpeg.SnapshotAsync(
                    tempVideoPath,
                    tempThumbnailPath,
                    captureTime: captureTime,
                    size: new System.Drawing.Size(1280, 720)
                );

                // Read thumbnail file as bytes
                return await File.ReadAllBytesAsync(tempThumbnailPath);
            }
            finally
            {
                // Cleanup temp files
                if (File.Exists(tempVideoPath))
                {
                    File.Delete(tempVideoPath);
                }
                if (File.Exists(tempThumbnailPath))
                {
                    File.Delete(tempThumbnailPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate video thumbnail");
            throw new InvalidOperationException("Failed to generate video thumbnail", ex);
        }
    }

    public async Task<VideoMetadata> GetVideoMetadataAsync(Stream videoStream)
    {
        try
        {
            // Save stream to temporary file (FFMpeg requires file path)
            var tempVideoPath = Path.GetTempFileName();

            try
            {
                // Write stream to temp file
                using (var fileStream = File.Create(tempVideoPath))
                {
                    await videoStream.CopyToAsync(fileStream);
                }

                // Analyze video
                var mediaInfo = await FFProbe.AnalyseAsync(tempVideoPath);

                var firstVideoStream = mediaInfo.VideoStreams.FirstOrDefault()
                    ?? throw new InvalidOperationException("No video stream found");

                return new VideoMetadata(
                    DurationSeconds: mediaInfo.Duration.TotalSeconds,
                    Width: firstVideoStream.Width,
                    Height: firstVideoStream.Height,
                    Format: mediaInfo.Format.FormatName
                );
            }
            finally
            {
                // Cleanup temp file
                if (File.Exists(tempVideoPath))
                {
                    File.Delete(tempVideoPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract video metadata");
            throw new InvalidOperationException("Failed to extract video metadata", ex);
        }
    }
}
