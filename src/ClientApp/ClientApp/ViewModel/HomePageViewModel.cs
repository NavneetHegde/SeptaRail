using CommunityToolkit.Maui.Views;
using SeptaRail.ClientApp.Models;
using SeptaRail.ClientApp.Services;
using SeptaRail.ClientApp.Views.Content;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;


namespace SeptaRail.ClientApp.ViewModel;

public class HomePageViewModel : INotifyPropertyChanged
{
    INextTrainFunction _nextTrainFunction;
    public string _fromPickerSelectedTrxt;
    public string _toPickerSelectedTrxt;
    public event PropertyChangedEventHandler PropertyChanged;
    private DateTime _dateTime;
    private Timer _timer;
    public List<NextTrain> _nextTrains = new List<NextTrain>();
    public List<string> _stationList = new List<string>();
    static Page Page => Application.Current?.MainPage ?? throw new NullReferenceException();

    public ICommand SearchTrainCommand { get; private set; }

    public HomePageViewModel(INextTrainFunction nextTrainFunction)
    {
        _nextTrainFunction = nextTrainFunction;
        this.DateTime = DateTime.Now;
        // Update the DateTime property every second.
        _timer = new Timer(new TimerCallback((s) => this.DateTime = DateTime.Now), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        LoadPicker();

        SearchTrainCommand = new Command(execute: () =>
        {
            SearchTrain();
        });
    }

    async void SearchTrain()
    {
        if (string.IsNullOrWhiteSpace(FromPickerSelectedText) || string.IsNullOrWhiteSpace(ToPickerSelectedText))
            return;

        var popup = new SpinnerPopup();
        Page.ShowPopup(popup);
        var nextTrainRequest = new NextTrainRequest
        {
            From = FromPickerSelectedText ?? string.Empty,
            To = ToPickerSelectedText ?? string.Empty
        };

        var trains = await _nextTrainFunction.GetTasksAsync(nextTrainRequest);
        int emptyTrainsCount = 3 - trains.Count();
        if (trains.Count() < 3) // Fill Empty 
        {
            for (int i = 0; i < emptyTrainsCount; i++)
            {
                trains.Add(new NextTrain
                {
                    isdirect = "true",
                    orig_arrival_time = "...",
                    orig_delay = "...",
                    orig_line = "...",
                    orig_departure_time = "...",
                    orig_train = "..."
                });
            }
        }
        NextTrains = trains;
        popup.Close();
    }

    public string FromPickerSelectedText
    {
        get
        {
            return _fromPickerSelectedTrxt;
        }
        set
        {
            if (_fromPickerSelectedTrxt != value)
            {
                _fromPickerSelectedTrxt = value;
            }
        }
    }

    public string ToPickerSelectedText
    {
        get
        {
            return _toPickerSelectedTrxt;
        }
        set
        {
            if (_toPickerSelectedTrxt != value)
            {
                _toPickerSelectedTrxt = value;
            }
        }
    }

    public DateTime DateTime
    {
        get => _dateTime;
        set
        {
            if (_dateTime != value)
            {
                _dateTime = value;
                OnPropertyChanged(nameof(DateTime));
            }
        }
    }

    public List<NextTrain> NextTrains
    {
        get => _nextTrains;
        set
        {
            if (_nextTrains != value)
            {
                _nextTrains = value;
                OnPropertyChanged(nameof(NextTrains));
            }
        }
    }

    public List<string> StationList
    {
        get => _stationList;
        set
        {
            if (_stationList != value)
            {
                _stationList = value;
                OnPropertyChanged(nameof(StationList));
            }
        }
    }

    public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void LoadPicker()
    {
        _stationList.Add("9th St");
        _stationList.Add("30th Street Station");
        _stationList.Add("49th St");
        _stationList.Add("Airport Terminal A");
        _stationList.Add("Airport Terminal B");
        _stationList.Add("Airport Terminal C-D");
        _stationList.Add("Airport Terminal E-F");
        _stationList.Add("Allegheny");
        _stationList.Add("Allen Lane");
        _stationList.Add("Ambler");
        _stationList.Add("Angora");
        _stationList.Add("Ardmore");
        _stationList.Add("Ardsley");
        _stationList.Add("Bala");
        _stationList.Add("Berwyn");
        _stationList.Add("Bethayres");
        _stationList.Add("Bridesburg");
        _stationList.Add("Bristol");
        _stationList.Add("Bryn Mawr");
        _stationList.Add("Carpenter");
        _stationList.Add("Chalfont");
        _stationList.Add("Chelten Avenue");
        _stationList.Add("Cheltenham");
        _stationList.Add("Chester TC");
        _stationList.Add("Chestnut Hill East");
        _stationList.Add("Chestnut Hill West");
        _stationList.Add("Churchmans Crossing");
        _stationList.Add("Claymont");
        _stationList.Add("Clifton-Aldan");
        _stationList.Add("Colmar");
        _stationList.Add("Conshohocken");
        _stationList.Add("Cornwells Heights");
        _stationList.Add("Crestmont");
        _stationList.Add("Croydon");
        _stationList.Add("Crum Lynne");
        _stationList.Add("Curtis Park");
        _stationList.Add("Cynwyd");
        _stationList.Add("Daylesford");
        _stationList.Add("Darby");
        _stationList.Add("Delaware Valley College");
        _stationList.Add("Devon");
        _stationList.Add("Downingtown");
        _stationList.Add("Doylestown");
        _stationList.Add("East Falls");
        _stationList.Add("Eastwick Station");
        _stationList.Add("Eddington");
        _stationList.Add("Eddystone");
        _stationList.Add("Elkins Park");
        _stationList.Add("Elm St");
        _stationList.Add("Elwyn Station");
        _stationList.Add("Exton");
        _stationList.Add("Fern Rock TC");
        _stationList.Add("Fernwood");
        _stationList.Add("Folcroft");
        _stationList.Add("Forest Hills");
        _stationList.Add("Ft Washington");
        _stationList.Add("Fortuna");
        _stationList.Add("Fox Chase");
        _stationList.Add("Germantown");
        _stationList.Add("Gladstone");
        _stationList.Add("Glenolden");
        _stationList.Add("Glenside");
        _stationList.Add("Gravers");
        _stationList.Add("Gwynedd Valley");
        _stationList.Add("Hatboro");
        _stationList.Add("Haverford");
        _stationList.Add("Highland Ave");
        _stationList.Add("Highland");
        _stationList.Add("Holmesburg Jct");
        _stationList.Add("Ivy Ridge");
        _stationList.Add("Jefferson Station");
        _stationList.Add("Jenkintown-Wyncote");
        _stationList.Add("Langhorne");
        _stationList.Add("Lansdale");
        _stationList.Add("Lansdowne");
        _stationList.Add("Lawndale");
        _stationList.Add("Levittown");
        _stationList.Add("Link Belt");
        _stationList.Add("Main St");
        _stationList.Add("Malvern");
        _stationList.Add("Manayunk");
        _stationList.Add("Marcus Hook");
        _stationList.Add("Market East");
        _stationList.Add("Meadowbrook");
        _stationList.Add("Media");
        _stationList.Add("Melrose Park");
        _stationList.Add("Merion");
        _stationList.Add("Miquon");
        _stationList.Add("Morton");
        _stationList.Add("Moylan-Rose Valley");
        _stationList.Add("Mt Airy");
        _stationList.Add("Narberth");
        _stationList.Add("Neshaminy Falls");
        _stationList.Add("New Britain");
        _stationList.Add("Newark");
        _stationList.Add("Noble");
        _stationList.Add("Norristown TC");
        _stationList.Add("North Broad St");
        _stationList.Add("North Hills");
        _stationList.Add("North Philadelphia");
        _stationList.Add("North Wales");
        _stationList.Add("Norwood");
        _stationList.Add("Olney");
        _stationList.Add("Oreland");
        _stationList.Add("Overbrook");
        _stationList.Add("Paoli");
        _stationList.Add("Penllyn");
        _stationList.Add("Pennbrook");
        _stationList.Add("Philmont");
        _stationList.Add("Primos");
        _stationList.Add("Prospect Park");
        _stationList.Add("Queen Lane");
        _stationList.Add("Radnor");
        _stationList.Add("Ridley Park");
        _stationList.Add("Rosemont");
        _stationList.Add("Roslyn");
        _stationList.Add("Rydal");
        _stationList.Add("Ryers");
        _stationList.Add("Secane");
        _stationList.Add("Sedgwick");
        _stationList.Add("Sharon Hill");
        _stationList.Add("Somerton");
        _stationList.Add("Spring Mill");
        _stationList.Add("St. Davids");
        _stationList.Add("St. Martins");
        _stationList.Add("Stenton");
        _stationList.Add("Strafford");
        _stationList.Add("Suburban Station");
        _stationList.Add("Swarthmore");
        _stationList.Add("Tacony");
        _stationList.Add("Temple U");
        _stationList.Add("Thorndale");
        _stationList.Add("Torresdale");
        _stationList.Add("Trenton");
        _stationList.Add("Trevose");
        _stationList.Add("Tulpehocken");
        _stationList.Add("University City");
        _stationList.Add("Upsal");
        _stationList.Add("Villanova");
        _stationList.Add("Wallingford");
        _stationList.Add("Warminster");
        _stationList.Add("Washington Lane");
        _stationList.Add("Wayne Jct");
        _stationList.Add("Wayne Station");
        _stationList.Add("West Trenton");
        _stationList.Add("Whitford");
        _stationList.Add("Willow Grove");
        _stationList.Add("Wilmington");
        _stationList.Add("Wissahickon");
        _stationList.Add("Wister");
        _stationList.Add("Woodbourne");
        _stationList.Add("Wyndmoor");
        _stationList.Add("Wynnefield Avenue");
        _stationList.Add("Wynnewood");
        _stationList.Add("Yardley");
    }

    ~HomePageViewModel() => _timer.Dispose();
}

