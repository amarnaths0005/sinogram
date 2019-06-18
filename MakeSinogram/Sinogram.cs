using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/// Class created by Staff and Students of BMS College of Engineering, Bangalore for the 
/// Sinogram creation project, Medical Electronics Department. June-July 2016
/// - Amarnath S, aka Avijnata, Staff, Research Associate, amarnaths.codeproject@gmail.com
/// - Students: Atheeth P, Kishore KS, Jagruthi A, Lakshmi SD, Mamatha K, Savitha VS, Sutapa B
/// 
/// 
/// Parallel Beam Tomography
namespace MakeSinogram
{
    /// <summary>
    /// Sinogram for Parallel Beam Tomography
    /// </summary>
    class Sinogram
    {
        int numberOfAngles = 180;

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

        public List<byte> pixels8OriginalRed
        {
            get;
            set;
        }

        public List<byte> pixels8OriginalGreen
        {
            get;
            set;
        }

        public List<byte> pixels8OriginalBlue
        {
            get;
            set;
        }

        public BitmapSource SinogramBmp
        {
            get;
            set;
        }

        public BitmapSource InvertedSinogramBmp
        {
            get;
            set;
        }

        List<byte> pixels8SinogramRed;
        List<byte> pixels8SinogramGreen;
        List<byte> pixels8SinogramBlue;

        // List of doubles in sinogram image.
        List<double> sinogramValues;

        byte[] pixelsToWrite;
        int sinogramWidth, sinogramHeight;
        string fileName;

        public Sinogram(string fName)
        {
            pixels8OriginalRed = new List<byte>();
            pixels8OriginalGreen = new List<byte>();
            pixels8OriginalBlue = new List<byte>();

            pixels8SinogramRed = new List<byte>();
            pixels8SinogramGreen = new List<byte>();
            pixels8SinogramBlue = new List<byte>();

            sinogramValues = new List<double>();

            fileName = fName;
        }

        public void ComputeSinogram()
        {
            ComputeSinogramValues();
            ComputeSinogramImage();
            UpdateSinogramImage();
        }

        private void ComputeSinogramValues()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            SquarePaddedImage pi = new SquarePaddedImage(fileName);
            BitmapSource padImgBmp = pi.SquareImage;

            List<byte> pixels8Red = pi.Pixels8PaddedRed;
            List<byte> pixels8Green = pi.Pixels8PaddedGreen;
            List<byte> pixels8Blue = pi.Pixels8PaddedBlue;

            SquareWidth = pi.PaddedWidth;
            SquareHeight = pi.PaddedHeight;

            int sinogramValuesSize = numberOfAngles * SquareWidth;
            sinogramWidth = numberOfAngles;
            sinogramHeight = SquareWidth;

            // Initialize the sinogram buffer
            sinogramValues.Clear();
            pixels8SinogramRed.Clear();
            pixels8SinogramGreen.Clear();
            pixels8SinogramBlue.Clear();

            for (int i = 0; i < sinogramValuesSize; ++i)
            {
                sinogramValues.Add(0.0);
                pixels8SinogramRed.Add(0);
                pixels8SinogramGreen.Add(0);
                pixels8SinogramBlue.Add(0);
            }

            byte red, green, blue;
            double gray;
            double sum = 0;
            double maxsum = sum;
            double angleDegrees;

            // Compute the maximum value
            maxsum = SquareWidth * 256;

            ImageRotation ir = new ImageRotation();
            ir.Pixels8OriginalRed = pixels8Red;
            ir.Pixels8OriginalGreen = pixels8Green;
            ir.Pixels8OriginalBlue = pixels8Blue;
            ir.SquareWidth = pi.PaddedWidth;
            ir.SquareHeight = pi.PaddedHeight;

            List<byte> pixels8RotatedRed = new List<byte>();
            List<byte> pixels8RotatedGreen = new List<byte>();
            List<byte> pixels8RotatedBlue = new List<byte>();

            int index1, index2;

            // Populate the sinogram buffer
            for (int k = 0; k < numberOfAngles; ++k)
            {
                angleDegrees = -k;
                // Just watch the console for large images. 
                // It should go on until 180.
                Console.Write(k + " "); 
                ir.RotateImage(angleDegrees);
                pixels8RotatedRed = ir.Pixels8RotatedRed;
                pixels8RotatedGreen = ir.Pixels8RotatedGreen;
                pixels8RotatedBlue = ir.Pixels8RotatedBlue;

                for (int i = 0; i < SquareWidth; ++i)
                {
                    sum = 0;
                    index1 = i * numberOfAngles + k;
                    for (int j = 0; j < SquareHeight; ++j)
                    {
                        index2 = j * SquareWidth + i;
                        red = pixels8RotatedRed[index2];
                        green = pixels8RotatedGreen[index2];
                        blue = pixels8RotatedBlue[index2];
                        gray = red * 0.3 + green * 0.59 + blue * 0.11;
                        gray /= maxsum;
                        sum += gray;
                    }
                    sinogramValues[index1] = Math.Exp(-sum);
                } 
            }
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// Function to compute the sinogram image.
        /// </summary>
        private void ComputeSinogramImage()
        {
            double max = sinogramValues.Max();
            double min = sinogramValues.Min();
            max += 0.00001;
            min -= 0.00001;
            Console.WriteLine("Max is " + max);
            Console.WriteLine("Min is " + min);
            double factor = 256.0 / (max - min);
            double value;
            byte valueByte;
            int index1, index2;

            for (int i = 0; i < SquareWidth; ++i)
            {
                index2 = i * numberOfAngles;
                for (int k = 0; k < numberOfAngles; ++k)
                {
                    index1 = index2 + k;
                    value = sinogramValues[index1] - min;
                    value *= factor;
                    valueByte = (byte)(255 - value);
                    pixels8SinogramRed[index1] = valueByte;
                    pixels8SinogramGreen[index1] = valueByte;
                    pixels8SinogramBlue[index1] = valueByte;
                }
            }
        }

        /// <summary>
        /// Updates both the sinogram image and negated sinogram image
        /// </summary>
        private void UpdateSinogramImage()
        {
            int bitsPerPixel = 24;
            int stride = (sinogramWidth * bitsPerPixel + 7) / 8;
            if (pixelsToWrite != null) Array.Clear(pixelsToWrite, 0, pixelsToWrite.Length);

            pixelsToWrite = new byte[stride * sinogramHeight];
            int i1;

            for (int i = 0; i < pixelsToWrite.Count(); i += 3)
            {
                i1 = i / 3;
                pixelsToWrite[i] = pixels8SinogramRed[i1];
                pixelsToWrite[i + 1] = pixels8SinogramGreen[i1];
                pixelsToWrite[i + 2] = pixels8SinogramBlue[i1];
            }

            SinogramBmp = BitmapSource.Create(sinogramWidth, sinogramHeight, 96, 96, PixelFormats.Rgb24,
                null, pixelsToWrite, stride);

            for (int i = 0; i < pixelsToWrite.Count(); i += 3)
            {
                i1 = i / 3;
                pixelsToWrite[i] = (byte)(255 - pixels8SinogramRed[i1]);
                pixelsToWrite[i + 1] = (byte)(255 - pixels8SinogramGreen[i1]);
                pixelsToWrite[i + 2] = (byte)(255 - pixels8SinogramBlue[i1]);
            }

            InvertedSinogramBmp = BitmapSource.Create(sinogramWidth, sinogramHeight, 96, 96, PixelFormats.Rgb24,
                null, pixelsToWrite, stride);
        }
    }
}
