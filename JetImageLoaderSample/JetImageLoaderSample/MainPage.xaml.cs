using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using JetImageLoaderSample.Resources;
using System.Collections.ObjectModel;

namespace JetImageLoaderSample
{
    public partial class MainPage : PhoneApplicationPage
    {

        public class Image
        {
            public string ImageUrl { get; set; }
        }

        public ObservableCollection<Image> ImagesList { get; private set; }
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            ImagesList = new ObservableCollection<Image>();

            DataContext = this;
            Loaded += MainPage_Loaded;    
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {            
            ImagesList.Add(new Image { ImageUrl = GetImageUrl("1.jpg") });
            ImagesList.Add(new Image { ImageUrl = GetImageUrl("2.jpg") });
            // and so on

            for (int i = 3; i <= 33; i++)
            {
                ImagesList.Add(new Image { ImageUrl = GetImageUrl(i + ".jpg") });
            }
        }

        private static string GetImageUrl(string imageFileName)
        {
            return "http://jetimageloader.artemzin.com/sample_images/" + imageFileName;
        }
    }
}