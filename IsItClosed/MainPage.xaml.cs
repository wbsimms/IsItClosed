using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IsItClosed
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;
        private StorageFile photoFile;
        private string PHOTO_FILE_NAME = "garage.jpg";
        private static string storageKey = "tFkjmym5qTXZSuA9UGHJlFWINjPB4Bcn4j1DVxAbz/7EQGnTz4P0sZZ21Eb49y6upWPZbbUOHlsA/Zxc2t5ciA==";
        private bool IsStarted = false;
        private CancellationTokenSource cancellationToken;

        public MainPage()
        {
            this.InitializeComponent();
            StartButton_OnClick(null,null);
        }

        private async void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsStarted)
            {
                cancellationToken = new CancellationTokenSource();
                IsStarted = true;
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();
                this.StartButton.Content = "Stop";
                try
                {
                    await PeriodicTask.Run(() =>
                    {
                        takePhoto_Click(null, null);
                    }, TimeSpan.FromMinutes(2), cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    IsStarted = false;
                    this.StartButton.Content = "Start";
                }
            }
            else
            {
                cancellationToken.Cancel();
                IsStarted = false;
                this.StartButton.Content = "Start";
            }

        }

        private async void takePhoto_Click(object sender, TextChangedEventArgs e)
        {

            photoFile = await KnownFolders.PicturesLibrary.CreateFileAsync(
                PHOTO_FILE_NAME, CreationCollisionOption.ReplaceExisting);
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
            await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

            IRandomAccessStream photoStream = await photoFile.OpenReadAsync();
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(photoStream);
            captureImage.Source = bitmap;
            photoStream.Dispose();

            using (Stream photoStream2 = await photoFile.OpenStreamForReadAsync())
            {
                await UploadFileToStorage(photoStream2, PHOTO_FILE_NAME);
            }
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName)
        {
            StorageCredentials storageCredentials = new StorageCredentials("devfoundriesdev", storageKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("isitclosed");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            try
            {
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
            catch (Exception e)
            {
            }
            return await Task.FromResult(true);
        }
    }
}
