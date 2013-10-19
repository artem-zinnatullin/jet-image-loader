
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace JetImageLoader
{
    public abstract class BaseJetImageLoaderConverter : IValueConverter
    {
        protected JetImageLoader JetImageLoader { get; private set; }

        protected BaseJetImageLoaderConverter()
        {
            var config = GetJetImageLoaderConfig();
            if (config == null) throw new ArgumentException("JetImageLoaderConfig can not be null");

            JetImageLoader = JetImageLoader.Instance;
            JetImageLoader.Initialize(config);
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageUrl = value as string;

            if (String.IsNullOrEmpty(imageUrl)) return null;

            var imageUri = new Uri(imageUrl);

            if (imageUri.Scheme == "http" || imageUri.Scheme == "https")
            {
                var bitmapImage = new BitmapImage();

                Task.Factory.StartNew(() => JetImageLoader.LoadImageStream(imageUri).ContinueWith(getImageStreamTask =>
                {
                    if (getImageStreamTask.Result != null)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() => 
                        {
                            try
                            {
                                bitmapImage.SetSource(getImageStreamTask.Result);
                            }
                            catch
                            {
                                // catching exceptions, like when source stream is corrupted or is not an image, etc...
                            }
                        });
                    }
                }));

                return bitmapImage;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
        protected abstract JetImageLoaderConfig GetJetImageLoaderConfig();
    }
}
