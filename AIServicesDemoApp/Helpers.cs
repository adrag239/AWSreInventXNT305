using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace AIServicesDemoApp;

public static class Helpers
{
    public static void DrawRectangleUsingBoundingBox(this Image image,
        Amazon.Rekognition.Model.BoundingBox boundingBox)
    {
        // Draw the rectangle using the bounding box values
        // They are percentages so scale them to picture
        image.Mutate(x => x.DrawLine(
            Rgba32.ParseHex("FF0000"),
            15,
            new PointF[]
            {
                new PointF(image.Width * boundingBox.Left, image.Height * boundingBox.Top),
                new PointF(image.Width * (boundingBox.Left + boundingBox.Width),
                    image.Height * boundingBox.Top),
                new PointF(image.Width * (boundingBox.Left + boundingBox.Width),
                    image.Height * (boundingBox.Top + boundingBox.Height)),
                new PointF(image.Width * boundingBox.Left,
                    image.Height * (boundingBox.Top + boundingBox.Height)),
                new PointF(image.Width * boundingBox.Left, image.Height * boundingBox.Top),
            }
        ));
    }

    public static void DrawRectangleUsingBoundingBox(this Image image,
        Amazon.Textract.Model.BoundingBox boundingBox)
    {
        // Draw the rectangle using the bounding box values
        // They are percentages so scale them to picture
        image.Mutate(x => x.DrawLine(
            Rgba32.ParseHex("FF0000"),
            15,
            new PointF[]
            {
                new PointF(image.Width * boundingBox.Left, image.Height * boundingBox.Top),
                new PointF(image.Width * (boundingBox.Left + boundingBox.Width),
                    image.Height * boundingBox.Top),
                new PointF(image.Width * (boundingBox.Left + boundingBox.Width),
                    image.Height * (boundingBox.Top + boundingBox.Height)),
                new PointF(image.Width * boundingBox.Left,
                    image.Height * (boundingBox.Top + boundingBox.Height)),
                new PointF(image.Width * boundingBox.Left, image.Height * boundingBox.Top),
            }
        ));
    }
}