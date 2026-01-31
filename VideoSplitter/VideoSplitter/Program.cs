// See https://aka.ms/new-console-template for more information

using NReco.VideoConverter;

var src = @"C:\Users\runiv\OneDrive\Billeder\New Zealand\P1000381.MP4";
var output = @"C:\workspaces\vormadal\video-tag\TestOutput";
var converter = new FFMpegConverter();

using var outputVideo = new FileStream(Path.Combine(output, "clip1.mp4"), FileMode.CreateNew);
using var outputThumbnail = new FileStream(Path.Combine(output, "clip1.jpeg"), FileMode.CreateNew);
//ffMpeg.GetVideoThumbnail

converter.ConvertMedia(src, null, outputVideo, Format.mp4, new ConvertSettings
{
    Seek = 5,
    MaxDuration = 5
});

converter.GetVideoThumbnail(src, outputThumbnail, 5);