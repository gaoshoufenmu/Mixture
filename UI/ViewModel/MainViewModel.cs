using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Input;
using System.Windows;
using UI.model;
using System.Collections.ObjectModel;

namespace UI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}


            FuckPrevCommand = new RelayCommand(FuckPrev);
            FuckNextCommand = new RelayCommand(FuckNext);
            FuckCommand = new RelayCommand(Fuck);
            SwitchCommand = new RelayCommand(Switch);
            GeneralVisibility = Visibility.Visible;
            SpecialVisibility = Visibility.Collapsed;
        }

        private ObservableCollection<VCompany> _companies;
        public ObservableCollection<VCompany> Companies
        {
            get { return _companies; }
            set
            {
                _companies = value;
                RaisePropertyChanged("Companies");
            }
        }

        public ICommand FuckPrevCommand { get; set; }
        public ICommand FuckNextCommand { get; set; }
        public ICommand FuckCommand { get; set; }
        public ICommand SwitchCommand { get; set; }
        private int _page = 0;
        public int Page
        {
            get { return _page; }
            set
            {
                _page = value;
                RaisePropertyChanged("Page");
            }
        }

        private int _generalIndex;
        public int GeneralIndex
        {
            get { return _generalIndex; }
            set
            {
                _generalIndex = value;
                RaisePropertyChanged("GeneralIndex");
            }
        }

        private int _specialIndex;
        public int SpecialIndex
        {
            get { return _specialIndex; }
            set
            {
                _specialIndex = value;
                RaisePropertyChanged(() => SpecialIndex);
            }
        }
        private bool _nextButtonEnabled;
        public bool NextButtonEnabled
        {
            get { return _nextButtonEnabled; }
            set
            {
                _nextButtonEnabled = value;
                RaisePropertyChanged("NextButtonEnabled");
            }
        }
        private bool _prevButtonEnabled;
        public bool PrevButtonEnabled
        {
            get { return _prevButtonEnabled; }
            set
            {
                _prevButtonEnabled = value;
                RaisePropertyChanged("PrevButtonEnabled");
            }
        }
        private string _noneVisibility;
        public string NoneVisibility
        {
            get { return _noneVisibility; }
            set
            {
                _noneVisibility = value;
                RaisePropertyChanged("NoneVisibility");
            }
        }

        private Visibility _generalVisibility;
        public Visibility GeneralVisibility
        {
            get { return _generalVisibility; }
            set
            {
                _generalVisibility = value;
                RaisePropertyChanged(() => this.GeneralVisibility);
            }
        }
        private Visibility _specialVisibility;
        public Visibility SpecialVisibility
        {
            get { return _specialVisibility; }
            set
            {
                _specialVisibility = value;
                RaisePropertyChanged(() => SpecialVisibility);
            }
        }

        private string _input;
        public string Input
        {
            get { return _input; }
            set
            {
                _input = value;
                RaisePropertyChanged("Input");
            }
        }
        private string _waitHint;
        public string WaitHint
        {
            get { return _waitHint; }
            set
            {
                _waitHint = value;
                RaisePropertyChanged("WaitHint");
            }
        }

        private void FuckPrev() { }
        private void FuckNext() { }
        private void Switch() { }
        private void Fuck() { }
    }
}