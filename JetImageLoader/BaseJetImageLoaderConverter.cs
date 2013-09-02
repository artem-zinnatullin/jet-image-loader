
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
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            var config = GetJetImageLoaderConfig();
            if (config == null) throw new ArgumentException("JetImageLoaderConfig can not be null");

            JetImageLoader = JetImageLoader.Instance;
            JetImageLoader.Initialize(config);
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageUrl = value as string;

            if (String.IsNullOrEmpty(imageUrl)) return null;

            Uri imageUri;

            try
            {
                imageUri = new Uri(imageUrl);
            }
            catch
            {
                JetImageLoader.Log("[network] error incorrect uri " + value + " , image was not loaded");
                return null;
            }

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
                                // Try to set source again
                                try
                                {
                                    bitmapImage.SetSource(getImageStreamTask.Result);
                                }
                                catch
                                {
                                    JetImageLoader.Log("[error] can not set image stream as source for BitmapImage, image uri: " + imageUrl);
                                }
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
