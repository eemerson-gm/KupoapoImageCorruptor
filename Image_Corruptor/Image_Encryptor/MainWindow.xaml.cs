using System;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

namespace Image_Encryptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Defines global image objects.
        BitmapImage globalImageOriginal;
        BitmapImage globalImageCorrupted;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {

            //Opens the file explorer dialog.
            using(OpenFileDialog fileDialog = new OpenFileDialog())
            {

                //Sets the dialog default values.
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                fileDialog.Filter = "Image Files (*.png)|*.png|Jpeg Files (*.jpg)|*.jpg";
                fileDialog.FilterIndex = 1;

                //Checks if a file from the dialog was selected.
                if(fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    //Sets the file path in the textbox.
                    TextboxFilename.Text = fileDialog.FileName;

                    //Reads the image from the file.
                    Bitmap fileBitmap = new Bitmap(fileDialog.FileName);

                    //Converts the bitmap image to the image source.
                    using(MemoryStream fileMemory = new MemoryStream())
                    {

                        //Saves the bitmap to memory.
                        fileBitmap.Save(fileMemory, System.Drawing.Imaging.ImageFormat.Png);

                        //Resets the memory position.
                        fileMemory.Position = 0;

                        //Creates a new bitmap image.
                        BitmapImage fileImage = new BitmapImage();

                        //Copies the memory over the the bitmap image.
                        fileImage.BeginInit();
                        fileImage.StreamSource = fileMemory;
                        fileImage.CacheOption = BitmapCacheOption.OnLoad;
                        fileImage.EndInit();

                        //Sets the image source in the menu.
                        ImagePreview.Source = fileImage;
                        globalImageOriginal = fileImage;
                        globalImageCorrupted = fileImage;

                        //Enables the password textbox.
                        TextboxPassword.IsEnabled = true;
                        TextboxPassword.MaxLength = globalImageOriginal.PixelWidth;

                    }

                }

            }

        }

        private void TextboxPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            CorruptImage();
        }

        private void RadioEncrypt_Click(object sender, RoutedEventArgs e)
        {
            CorruptImage();
        }

        private void RadioDecrypt_Click(object sender, RoutedEventArgs e)
        {
            CorruptImage();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            //Opens a save file dialog.
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = "Untitled";
            saveDialog.Filter = "Image Files (*.png)|*.png|Jpeg Files (*.jpg)|*.jpg";
            saveDialog.FilterIndex = 1;

            //Checks if the save was successful.
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Bitmap saveBitmap = ConvertBitmapImageToBitmap(globalImageCorrupted);
                saveBitmap.Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        public BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }

        public Bitmap ConvertBitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public void CorruptImage()
        {

            //Gets a writeable bitmap from the source image.
            WriteableBitmap corruptBitmap = new WriteableBitmap(globalImageOriginal);

            //Locks the bitmap for writing.
            corruptBitmap.Lock();

            //Gets the bitmap image stride.
            int corruptStride = (int)corruptBitmap.PixelWidth * (corruptBitmap.Format.BitsPerPixel / 8);
            int corruptLength = (int)corruptBitmap.PixelHeight * corruptStride;

            //Gets a array of pixel bytes.
            byte[] corruptPixels = new byte[corruptLength];

            //Copies the pixels to the array.
            globalImageOriginal.CopyPixels(corruptPixels, corruptStride, 0);

            //Checks if the password is not empty.
            if (TextboxPassword.Text != "")
            {

                //Gets a color counter.
                int corruptColor = 0;

                //Gets a password counter.
                int corruptPassword = 0;

                //Checks if corruption is checked.
                if (RadioCorruptMin.IsChecked == true)
                {

                    //Loops through each byte in the array.
                    for (int p = 0; p < corruptPixels.Length; p++)
                    {
                        //Checks the corruption color index.
                        if(corruptColor <= 2)
                        {
                            int c = corruptPixels[p];
                            c -= TextboxPassword.Text[corruptPassword];
                            c = (((c - 0) % (255 - 0)) + (255 - 0)) % (255 - 0) + 0;
                            corruptPixels[p] = (byte)c;
                            corruptColor++;
                        }
                        else
                        {
                            corruptColor = 0;
                            corruptPassword++;
                            if (corruptPassword >= TextboxPassword.Text.Length)
                            {
                                corruptPassword = 0;
                            }
                        }
                    }

                }
                else
                {

                    //Loops through each byte in the array.
                    for (int p = 0; p < corruptPixels.Length; p++)
                    {

                        //Checks the corruption color index.
                        if (corruptColor <= 2)
                        {
                            int c = corruptPixels[p];
                            c *= TextboxPassword.Text[corruptPassword];
                            c = (((c - 0) % (255 - 0)) + (255 - 0)) % (255 - 0) + 0;
                            corruptPixels[p] = (byte)c;
                            corruptColor++;
                        }
                        else
                        {
                            corruptColor = 0;
                            corruptPassword++;
                            if(corruptPassword >= TextboxPassword.Text.Length)
                            {
                                corruptPassword = 0;
                            }
                        }

                    }

                }

            }

            //Writes the pixel back into the image.
            corruptBitmap.WritePixels(new Int32Rect(0, 0, corruptBitmap.PixelWidth, corruptBitmap.PixelHeight), corruptPixels, corruptStride, 0);

            //Unlocks the bitmap image.
            corruptBitmap.Unlock();

            //Sets the corrupted image source.
            globalImageCorrupted = ConvertWriteableBitmapToBitmapImage(corruptBitmap);
            ImagePreview.Source = globalImageCorrupted;

        }
    }

}
