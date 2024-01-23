using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Webcam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCaptureDevice videoSource;
        public MainWindow()
        {
            InitializeComponent();
            InitializeCamera();
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }
        #region method
        private void InitializeCamera()
        {
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No video devices found.");
                return;
            }

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);
            videoSource.Start();
        }
        private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                Dispatcher.Invoke(() =>
                {
                    webcamImage.Source = ConvertBitmapToBitmapSource(bitmap);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying video: {ex.Message}");
            }
        }

        private BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            videoSource?.Stop();
        }
        private string GenerateRandomString()
        {
            const string chars = "0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion

        #region event
        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)webcamImage.ActualWidth,
                                                                                (int)webcamImage.ActualHeight,
                                                                                96, 96, PixelFormats.Default);

                renderTargetBitmap.Render(webcamImage);
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        string selectedFolder = dialog.SelectedPath;
                        string filePath = selectedFolder + "\\" + GenerateRandomString() + ".png";
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }
                        MessageBox.Show("Hình ảnh đã được chụp và lưu thành công!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /*private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            }
        }*/
        #endregion
    }
}