using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

/// Class created by Staff and Students of BMS College of Engineering, Bangalore for the 
/// Sinogram creation project, Medical Electronics Department. June-July 2016
/// - Amarnath S, aka Avijnata, Staff, Research Associate, amarnaths.codeproject@gmail.com
/// - Students: Atheeth P, Kishore KS, Jagruthi A, Lakshmi SD, Mamatha K, Savitha VS, Sutapa B
/// Parallel Beam Tomography
namespace MakeSinogram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapImage bmpImg;
        string fileName = "";
        Sinogram sinogram;
        bool inverted;

        public MainWindow()
        {
            InitializeComponent();

            bnSaveSino.IsEnabled = false;
            bnSinogram.IsEnabled = false;
            bnInvertSino.IsEnabled = false;

            inverted = false;
        }
        
        /// <summary>
        /// Opening the image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.bmp;*.png)|*.bmp;*.png|24-Bit Bitmap(*.bmp)|*.bmp|PNG(*.png)|*.png";

            Nullable<bool> result = ofd.ShowDialog();

            if (result == true)
            {
                fileName = ofd.FileName;

                imageSource.Source = new BitmapImage(new Uri(fileName, UriKind.Absolute));

                bmpImg = new BitmapImage();
                bmpImg.BeginInit();
                bmpImg.UriSource = new Uri(fileName, UriKind.Absolute);
                bmpImg.EndInit();

                bnSinogram.IsEnabled = true;
                imageSinogram.Source = null;
                bnInvertSino.IsEnabled = false;
                bnSaveSino.IsEnabled = false;
            }
        }

        /// <summary>
        /// Button click for sinogram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void generateSinogram_Click(object sender, RoutedEventArgs e)
        {
            sinogram = new Sinogram(fileName);
            sinogram.ComputeSinogram();
            imageSinogram.Source = sinogram.SinogramBmp;

            bnSaveSino.IsEnabled = true;
            bnInvertSino.IsEnabled = true;
        }

        /// <summary>
        /// Function to save the sinogram image 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG Files(*.png)|*.png";

            if (sfd.ShowDialog() == true)
            {
                if (sinogram != null)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(sinogram.SinogramBmp as BitmapSource));
                    encoder.Save(fs);
                    fs.Close();
                }
                else
                {
                    MessageBox.Show("Please generate the sinogram before saving it!");
                }
            }
        }

        private void invert_Click(object sender, RoutedEventArgs e)
        {
            if (!inverted)
            {
                imageSinogram.Source = sinogram.InvertedSinogramBmp;
            }
            else
            {
                imageSinogram.Source = sinogram.SinogramBmp;
            }
            inverted = !inverted;
        }
    }
}
