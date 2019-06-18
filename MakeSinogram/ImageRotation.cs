using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/// Class created by Staff and Students of BMS College of Engineering, Bangalore for the 
/// Sinogram creation project, Medical Electronics Department. June-July 2016
/// - Amarnath S, aka Avijnata, Staff, Research Associate, amarnaths.codeproject@gmail.com
/// - Students: Atheeth P, Kishore KS, Jagruthi A, Lakshmi SD, Mamatha K, Savitha VS, Sutapa B
/// Parallel Beam Tomography
namespace MakeSinogram
{
    /// <summary>
    /// Class to do image rotation. While there are several image operations possible
    /// we focus on image rotation only, as that is what is needed for creation of 
    /// the sinogram.
    /// </summary>
    class ImageRotation
    {
        public int SquareWidth
        {
            get;
            set;
        }

        public int SquareHeight
        {
            get;
            set;
        }

        public List<byte> Pixels8OriginalRed
        {
            get;
            set;
        }

        public List<byte> Pixels8OriginalGreen
        {
            get;
            set;
        }

        public List<byte> Pixels8OriginalBlue
        {
            get;
            set;
        }

        public List<byte> Pixels8RotatedRed
        {
            get;
            set;
        }

        public List<byte> Pixels8RotatedGreen
        {
            get;
            set;
        }

        public List<byte> Pixels8RotatedBlue
        {
            get;
            set;
        }

        public BitmapSource RotatedImage
        {
            get;
            set;
        }

        public double AngleRotation // Degrees
        {
            get;
            set;
        }

        byte[] pixelsToWrite;

        public ImageRotation()
        {
            Pixels8OriginalRed = new List<byte>();
            Pixels8OriginalGreen = new List<byte>();
            Pixels8OriginalBlue = new List<byte>();

            Pixels8RotatedRed = new List<byte>();
            Pixels8RotatedGreen = new List<byte>();
            Pixels8RotatedBlue = new List<byte>();
        }

        public void RotateAndUpdateImage()
        {
            RotateImageBilinearInterpolation(AngleRotation);
            UpdatePaddedImage();
        }

        /// <summary>
        /// Function to rotate the image by the specified angle
        /// </summary>
        public void RotateImage(double angleRotation)
        {
            RotateImageBilinearInterpolation(angleRotation);
        }

        /// <summary>
        /// Function to create the bitmap of the rotated image
        /// </summary>
        private void UpdatePaddedImage()
        {
            int bitsPerPixel = 24;
            int stride = (SquareWidth * bitsPerPixel + 7) / 8;
            if (pixelsToWrite != null) Array.Clear(pixelsToWrite, 0, pixelsToWrite.Length);

            pixelsToWrite = new byte[stride * SquareHeight];
            int i1;

            for (int i = 0; i < pixelsToWrite.Count(); i += 3)
            {
                i1 = i / 3;
                pixelsToWrite[i] = Pixels8RotatedRed[i1];
                pixelsToWrite[i + 1] = Pixels8RotatedGreen[i1];
                pixelsToWrite[i + 2] = Pixels8RotatedBlue[i1];
            }

            RotatedImage = BitmapSource.Create(SquareWidth, SquareHeight, 96, 96, PixelFormats.Rgb24,
                null, pixelsToWrite, stride);
        }

        /// <summary>
        /// Code to rotate an image. Adapted from the code of Vincent Tan, available on Internet. 
        /// </summary>
        /// <param name="angleDegrees"></param>
        private void RotateImageBilinearInterpolation(double angleDegrees)
        {
            // Clear buffers
            Pixels8RotatedRed.Clear();
            Pixels8RotatedGreen.Clear();
            Pixels8RotatedBlue.Clear();

            int width = SquareWidth;
            int height = SquareHeight;

            double xcentre = (double)(width) / 2.0;
            double ycentre = (double)(height) / 2.0;

            double x, y, dist, polarAngle, trueX, trueY, floorX, floorY, ceilX, ceilY;
            int topLeftIndex, topRightIndex, bottomLeftIndex, bottomRightIndex;
            double topRed, topGreen, topBlue, bottomRed, bottomGreen, bottomBlue;
            double deltaX, deltaY;
            int red, green, blue;
            int targetIndex, index1;
            double factor = Math.PI / 180.0;
            double angle1 = 1.5 * Math.PI;
            double angle2 = 0.5 * Math.PI;

            for (int j = 0; j < height * width; ++j)
            {
                Pixels8RotatedRed.Add(0);
                Pixels8RotatedGreen.Add(0);
                Pixels8RotatedBlue.Add(0);
            }

            byte background = 0; // black

            for (int j = 0; j < height; ++j)
            {
                index1 = j * width;
                for (int i = 0; i < width; ++i)
                {
                    // Coordinates with respect to centre of image
                    x = i - xcentre;
                    y = ycentre - j;

                    targetIndex = index1 + i;

                    Pixels8RotatedRed[targetIndex] = background;
                    Pixels8RotatedGreen[targetIndex] = background;
                    Pixels8RotatedBlue[targetIndex] = background;

                    dist = Math.Sqrt(x * x + y * y);
                    polarAngle = 0.0;

                    if (x == 0)
                    {
                        if (y == 0)
                        {
                            Pixels8RotatedRed[targetIndex] = Pixels8OriginalRed[targetIndex];
                            Pixels8RotatedGreen[targetIndex] = Pixels8OriginalGreen[targetIndex];
                            Pixels8RotatedBlue[targetIndex] = Pixels8OriginalBlue[targetIndex];
                        }
                        else if (y < 0)
                        {
                            polarAngle = angle1; // 1.5 * Math.PI;
                        }
                        else
                        {
                            polarAngle = angle2; //  0.5 * Math.PI;
                        }
                    }
                    else
                    {
                        polarAngle = Math.Atan2((double)y, (double)x);
                    }

                    polarAngle -= (angleDegrees * factor);

                    // Polar to Cartesian
                    trueX = dist * Math.Cos(polarAngle);
                    trueY = dist * Math.Sin(polarAngle);

                    // Coordinates with respect to top left corner of image
                    trueX = trueX + (double)xcentre;
                    trueY = (double)ycentre - trueY;

                    floorX = (int)(Math.Floor(trueX));
                    floorY = (int)(Math.Floor(trueY));
                    ceilX = (int)(Math.Ceiling(trueX));
                    ceilY = (int)(Math.Ceiling(trueY));

                    // Checking of bounds
                    if (floorX < 0 || ceilX < 0 ||
                        floorX >= SquareWidth || ceilX >= SquareWidth||
                        floorY < 0 || ceilY < 0 ||
                        floorY >= SquareHeight || ceilY >= SquareHeight ) continue;

                    deltaX = trueX - (double)floorX;
                    deltaY = trueY - (double)floorY;

                    topLeftIndex = Convert.ToInt32(floorY * SquareHeight + floorX);
                    topRightIndex = Convert.ToInt32(floorY * SquareHeight + ceilX);
                    bottomLeftIndex = Convert.ToInt32(ceilY * SquareHeight + floorX);
                    bottomRightIndex = Convert.ToInt32(ceilY * SquareHeight + ceilX);

                    // Linear interpolation - horizontal between top neighbours
                    topRed = (1 - deltaX) * Pixels8OriginalRed[topLeftIndex] +
                        deltaX * Pixels8OriginalRed[topRightIndex];
                    topGreen = (1 - deltaX) * Pixels8OriginalGreen[topLeftIndex] +
                        deltaX * Pixels8OriginalGreen[topRightIndex];
                    topBlue = (1 - deltaX) * Pixels8OriginalBlue[topLeftIndex] +
                        deltaX * Pixels8OriginalBlue[topRightIndex];

                    // Linear interpolation - horizontal between bottom neighbours
                    bottomRed = (1 - deltaX) * Pixels8OriginalRed[bottomLeftIndex] +
                        deltaX * Pixels8OriginalRed[bottomRightIndex];
                    bottomGreen = (1 - deltaX) * Pixels8OriginalGreen[bottomLeftIndex] +
                        deltaX * Pixels8OriginalGreen[bottomRightIndex];
                    bottomBlue = (1 - deltaX) * Pixels8OriginalBlue[bottomLeftIndex] +
                        deltaX * Pixels8OriginalBlue[bottomRightIndex];

                    // Linear interpolation - vertical between top and bottom values
                    red = (int)(Math.Round((1 - deltaY) * topRed + deltaY * bottomRed));
                    green = (int)(Math.Round((1 - deltaY) * topGreen + deltaY * bottomGreen));
                    blue = (int)(Math.Round((1 - deltaY) * topBlue + deltaY * bottomBlue));

                    // Clamping of colours
                    if (red < 0) red = 0;
                    if (red > 255) red = 255;
                    if (green < 0) green = 0;
                    if (green > 255) green = 255;
                    if (blue < 0) blue = 0;
                    if (blue > 255) blue = 255;

                    Pixels8RotatedRed[targetIndex] = (byte)red;
                    Pixels8RotatedGreen[targetIndex] = (byte)green;
                    Pixels8RotatedBlue[targetIndex] = (byte)blue;
                }
            }
        }    
    }
}
