using System;
using eaio.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace eaio.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {

        public RelayCommand HomeViewCommand { get; set; }

        public RelayCommand TranscoderViewCommand { get; set; }

        public RelayCommand InterpolationViewCommand { get; set; }

        public RelayCommand UpscalerViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }

        public TranscoderViewModel TranscoderVM { get; set; }

        public InterpolationViewModel InterpolationVM { get; set; }

        public UpscalerViewModel UpscalerVM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set 
            { 
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            HomeVM = new HomeViewModel();
            TranscoderVM = new TranscoderViewModel();
            InterpolationVM = new InterpolationViewModel();
            UpscalerVM = new UpscalerViewModel();

            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVM;
            });

            TranscoderViewCommand = new RelayCommand(o =>
            {
                CurrentView = TranscoderVM;
            });

            InterpolationViewCommand = new RelayCommand(o =>
            {
                CurrentView = InterpolationVM;
            });

            UpscalerViewCommand = new RelayCommand(o =>
            {
                CurrentView = UpscalerVM;
            });
        }
    }
}
