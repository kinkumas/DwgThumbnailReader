using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace DwgThumbnailViewer
{
    public partial class ThumbnailPanel : UserControl
    {

        public ImageSource? ThumbnailImg
        {
            get => Img.Source;
            set => Img.Source = value;
        }

        public string ThumbnailTitle
        {
            get => Title.Text;
            set => Title.Text = value;
        }

        public ThumbnailPanel()
        {
            InitializeComponent();
        }
    }
}
