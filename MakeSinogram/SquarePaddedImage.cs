using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
    /// Class for converting a rectangular image to a square image. 
    /// The extra pixels are padded as black.
    /// </summary>
    class SquarePaddedImage
    {
        int originalWidth;
        int originalHeight;
        int bitsPerPixel;
        byte background = 0; // black
        int bufferSizePadded;

        byte[] originalPixels;
        byte[] pixelsToWrite;

        List<byte> pixels8OriginalRed;
        List<byte> pixels8OriginalGreen;
        List<byte> pixels8OriginalBlue;

        BitmapSource originalImage;

        public SquarePaddedImage(string fileName)
        {
            pixels8OriginalRed = new List<byte>();
            pixels8OriginalGreen = new List<byte>();
            pixels8OriginalBlue = new List<byte>();

            Pixels8PaddedRed = new List<byte>();
            Pixels8PaddedGreen = new List<byte>();
            Pixels8PaddedBlue = new List<byte>();

            ReadAndPadImage(fileName);
        }

        private void ReadAndPadImage(string fileName) {
            try
            {
                ReadOriginalImage(fileName);
                ComputePaddedImage();
            }
            catch (Exception)
            {
                MessageBox.Show("Sorry, this does not seem to be an image. Please open an image!");
            }
        }

        public int PaddedWidth
        {
            get;
            set;
        }

        public int PaddedHeight
        {
            get;
            set;
        }

        /// <summary>
        /// List of red pixels of padded image
        /// </summary>
        public List<byte> Pixels8PaddedRed
        {
            get;
            set;
        }

        /// <summary>
        /// List of green pixels of padded image
        /// </summary>
        public List<byte> Pixels8PaddedGreen
        {
            get;
            set;
        }

        /// <summary>
        /// List of blue pixels of padded image
        /// </summary>
        public List<byte> Pixels8PaddedBlue
        {
            get;
            set;
        }

        /// <summary>
        /// Bitmap containing the padded image
        /// </summary>
        public BitmapSource SquareImage
        {
            get;
            set;
        }

        /// <summary>
        /// Initialize the lists after clearing them
        /// </summary>
        void InitializePaddedPixels()
        {
            Pixels8PaddedRed.Clear();
            Pixels8PaddedGreen.Clear();
            Pixels8PaddedBlue.Clear();

            bufferSizePadded = PaddedWidth * PaddedHeight;

            for (int i = 0; i < bufferSizePadded; ++i)
            {
                Pixels8PaddedRed.Add(background);
                Pixels8PaddedGreen.Add(background);
                Pixels8PaddedBlue.Add(background);
            }
        }

        /// <summary>
        /// This is the main function of this class. Computes the padded 
        /// image given the original image.
        /// </summary>
        void ComputePaddedImage()
        {
            // First compute the padded width and height
            // and then populate the pixels.
            if (originalWidth == originalHeight)
            {
                PaddedWidth = originalWidth;
                PaddedHeight = originalHeight;
                Pixels8PaddedRed = pixels8OriginalRed;
                Pixels8PaddedGreen = pixels8OriginalGreen;
                Pixels8PaddedBlue = pixels8OriginalBlue;
            }
            else if (originalWidth > originalHeight)
            {
                // Width is greater than height; so we pad background pixels at the top and bottom
                // of the image, so that the original image comes in the middle of the square block
                PaddedWidth = originalWidth;
                PaddedHeight = originalWidth;
                InitializePaddedPixels();

                int diff = (PaddedHeight - originalHeight) / 2;

                for (int i = diff * PaddedWidth; i < diff * PaddedWidth + originalWidth * originalHeight; ++i)
                {
                    Pixels8PaddedRed[i] = pixels8OriginalRed[i - diff * PaddedWidth];
                    Pixels8PaddedGreen[i] = pixels8OriginalGreen[i - diff * PaddedWidth];
                    Pixels8PaddedBlue[i] = pixels8OriginalBlue[i - diff * PaddedWidth];
                }
            }
            else // if (originalWidth < originalHeight)
            {
                PaddedWidth = originalHeight;
                PaddedHeight = originalHeight;
                InitializePaddedPixels();

                int diff = (PaddedWidth - originalWidth) / 2;

                for (int i = 0; i < PaddedWidth * PaddedHeight; ++i)
                {
                    int i1 = i % PaddedWidth;

                    if ((i1 >= diff) && (i1 < (diff + originalWidth)))
                    {
                        int rowNumber = i / PaddedWidth;
                        Pixels8PaddedRed[i] = pixels8OriginalRed[originalWidth * rowNumber + i1 - diff];
                        Pixels8PaddedGreen[i] = pixels8OriginalGreen[originalWidth * rowNumber + i1 - diff];
                        Pixels8PaddedBlue[i] = pixels8OriginalBlue[originalWidth * rowNumber + i1 - diff];
                    }
                }
            }
            UpdatePaddedImage();            
        }

        /// <summary>
        /// Function to create a bitmap from the red, green and blue padded pixel buffers
        /// </summary>
        private void UpdatePaddedImage()
        {
            int bitsPerPixel = 24;
            int stride = (PaddedWidth * bitsPerPixel + 7) / 8;
            if (pixelsToWrite != null) Array.Clear(pixelsToWrite, 0, pixelsToWrite.Length);

            pixelsToWrite = new byte[stride * PaddedHeight];
            int i1;

            for (int i = 0; i < pixelsToWrite.Count(); i += 3)
            {
                i1 = i / 3;
                pixelsToWrite[i] = Pixels8PaddedRed[i1];
                pixelsToWrite[i + 1] = Pixels8PaddedGreen[i1];
                pixelsToWrite[i + 2] = Pixels8PaddedBlue[i1];
            }

            SquareImage = BitmapSource.Create(PaddedWidth, PaddedHeight, 96, 96, PixelFormats.Rgb24,
                null, pixelsToWrite, stride);
        }

        /// <summary>
        /// Function to read in the original image
        /// </summary>
        /// <param name="fileName"></param>
        void ReadOriginalImage(string fileName)
        {
            Uri imageUri = new Uri(fileName, UriKind.RelativeOrAbsolute);
            if (originalImage != null) originalImage = null;
            originalImage = new BitmapImage(imageUri);
            int stride = (originalImage.PixelWidth * originalImage.Format.BitsPerPixel + 7) / 8;
            originalWidth = originalImage.PixelWidth;
            originalHeight = originalImage.PixelHeight;
            bitsPerPixel = originalImage.Format.BitsPerPixel;

            byte red, green, blue;
            int bufferSize;

            if (originalImage.Format == PixelFormats.Bgr32 || originalImage.Format == PixelFormats.Bgra32)
            {
                // Clear existing buffers
                if (originalPixels != null) Array.Clear(originalPixels, 0, originalPixels.Length);
                pixels8OriginalRed.Clear();
                pixels8OriginalGreen.Clear();
                pixels8OriginalBlue.Clear();

                // Now read in the image
                bufferSize = stride * originalHeight;
                originalPixels = new byte[bufferSize];

                // Read in pixel values from the image
                originalImage.CopyPixels(Int32Rect.Empty, originalPixels, stride, 0);

                // Populate the Red, Green and Blue lists.
                if (bitsPerPixel == 32) // 32 bits per pixel
                {
                    for (int i = 0; i < originalPixels.Count(); i += 4)
                    {
                        // In a 32-bit per pixel image, the bytes are stored in the order 
                        // BGR - Blue Green Red Alpha order.
                        blue = (byte)(originalPixels[i]);
                        green = (byte)(originalPixels[i + 1]);
                        red = (byte)(originalPixels[i + 2]);

                        pixels8OriginalRed.Add(red);
                        pixels8OriginalGreen.Add(green);
                        pixels8OriginalBlue.Add(blue);
                    }
                }
                // Populate the Red, Green and Blue lists.
                else if (bitsPerPixel == 24) // 24 bits per pixel
                {
                    for (int i = 0; i < originalPixels.Count(); i += 3)
                    {
                        // In a 24-bit per pixel image, the bytes are stored in the order 
                        // BGR - Blue Green Red order.
                        blue = (byte)(originalPixels[i]);
                        green = (byte)(originalPixels[i + 1]);
                        red = (byte)(originalPixels[i + 2]);

                        pixels8OriginalRed.Add(red);
                        pixels8OriginalGreen.Add(green);
                        pixels8OriginalBlue.Add(blue);
                    }
                }
                else
                {
                    MessageBox.Show("Sorry, I am unable to open this image!");
                }
            }
        }
    }
}