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
        protected virtual JetImageLoader JetImageLoader { get; set; }

        protected BaseJetImageLoaderConverter()
        {
            var config = GetJetImageLoaderConfig();

            if (config == null)
            {
                throw new ArgumentException("JetImageLoaderConfig can not be null");
            }

            JetImageLoader = JetImageLoader.Instance;
            JetImageLoader.Initialize(config);
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // hack to hide warning "Unable to determine application identity of the caller"
            if (System.ComponentModel.DesignerProperties.IsInDesignTool)
                return null;

            Uri imageUri;

            if (value == null)
            {
                return new BitmapImage { UriSource = new Uri(Constants.RESOURCE_IMAGE_EMPTY_PRODUCT, UriKind.Relative) };
            }
            if (value is string)
            {
                try
                {
                    if (String.IsNullOrEmpty(value as string))
                        return new BitmapImage { UriSource = new Uri(Constants.RESOURCE_IMAGE_EMPTY_PRODUCT, UriKind.Relative) };

                    imageUri = new Uri((string)value);

                    if (imageUri.Scheme == "file")
                        return new BitmapImage { UriSource = new Uri(imageUri.LocalPath, UriKind.Relative) };
                }
                catch
                {
                    // TODO add error log or callback
                    return null;
                }
            }
            else if (value is Uri)
            {
                imageUri = (Uri)value;
            }
            else
            {
                // TODO add error log or callback
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
                                bitmapImage.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                            }
                            catch
                            {
                                // catching exceptions, like when source stream is corrupted or is not an image, etc...
                            }
                        });
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            bitmapImage.UriSource = new Uri(Constants.RESOURCE_IMAGE_EMPTY_PRODUCT, UriKind.Relative);
                            bitmapImage.CreateOptions = BitmapCreateOptions.BackgroundCreation;
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