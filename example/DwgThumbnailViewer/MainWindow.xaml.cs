using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using DwgThumbnailReader;

namespace DwgThumbnailViewer;

public partial class MainWindow : Window
{
    private string _dwgPath = "";

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void DirButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var fd = new OpenFolderDialog();
            fd.Title = "Select Dwg File Folder";

            if (fd.ShowDialog() != true) return;

            _dwgPath = fd.FolderName;

            ImgList.Children.Clear();

            await ShowThumbnail(_dwgPath);
        }
        catch (Exception error)
        {
            _ = error;
        }
    }

    private async void ReloadButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await ShowThumbnail(_dwgPath);
        }
        catch (Exception error)
        {
            _ = error;
        }
    }

    private async Task ShowThumbnail(string dir)
    {
        ImgList.Children.Clear();

        await Task.Run((() =>
        {
            foreach (var path in Directory.GetFileSystemEntries(dir, "*.dwg"))
            {
                var thumbnail = DwgThumbnail.GetImage(path);

                if (thumbnail.ImageType == DwgThumbnailImageType.None)
                    continue;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var thumb = new ThumbnailPanel();
                    thumb.Margin = new Thickness(5);
                    thumb.Width = 200;
                    thumb.Height = 200;
                    thumb.ThumbnailTitle = Path.GetFileName(path);
                    thumb.ThumbnailImg = LoadImage(thumbnail.Bytes.ToArray());

                    ImgList.Children.Add(thumb);
                });
            }
        }));
    }

    private static ImageSource? LoadImage(byte[] bytes)
    {
        try
        {
            using var ms = new MemoryStream(bytes);
            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.CreateOptions = BitmapCreateOptions.None;
            img.StreamSource = ms;
            img.EndInit();
            img.Freeze();

            return img;
        }
        catch (Exception e)
        {
            _ = e;
            return null;
        }
    }
}
