using Microsoft.Maui.Layouts; // Add this using if not present

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel _viewModel;
        private const double NarrowScreenWidth = 600;

        public MainPage()
        {
            InitializeComponent();
            _viewModel = new MainPageViewModel();
            BindingContext = _viewModel;
            RequestBluetoothPermissionsAsync();
            
            // Handle window size changes
            //this.SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, EventArgs e)
        {
            // If screen width is less than 600, allow buttons to wrap to new row
            if (this.Width < NarrowScreenWidth)
            {

                FlexLayout.SetBasis(startButton, new FlexBasis(1f, true));
                FlexLayout.SetBasis(stopButton, new FlexBasis(1f, true));
            }
            else
            {
                FlexLayout.SetBasis(startButton, new FlexBasis(0, false));
                FlexLayout.SetBasis(stopButton, new FlexBasis(0, false));
                startButton.HorizontalOptions = LayoutOptions.Start;
                stopButton.HorizontalOptions = LayoutOptions.Start;
            }
        }

        private async Task RequestBluetoothPermissionsAsync()
        {
#if ANDROID
            try
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
                {
                    var scanStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();
                    if (scanStatus != PermissionStatus.Granted)
                    {
                        await DisplayAlertAsync("Permission Required", "Bluetooth permissions are required to use this app", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Permission request error: {ex.Message}");
            }
#endif
        }

        private async void OnStartClicked(object sender, EventArgs e)
        {
           
        }

        private async void OnStopClicked(object sender, EventArgs e)
        {
           
        }
    }
}
