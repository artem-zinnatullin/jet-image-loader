using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetImageLoaderSample.Annotations;

namespace JetImageLoaderSample.ViewModels
{
    public class Image
    {
        public string ImageUrl { get; set; }
    }
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Image> ImagesList { get; private set; }


        public MainPageViewModel()
        {
            ImagesList = new ObservableCollection<Image>();
            ImagesList.Add(new Image { ImageUrl = GetImageUrl("1.jpg") });
            ImagesList.Add(new Image { ImageUrl = GetImageUrl("2.jpg") });

//            for (int i = 3; i <= 33; i++)
//            {
//                ImagesList.Add(new Image { ImageUrl = GetImageUrl(i + ".jpg") });
//            }
        }

        private string GetImageUrl(string imageFileName)
        {
            return "http://jetimageloader.artemzin.com/sample_images/" + imageFileName;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
