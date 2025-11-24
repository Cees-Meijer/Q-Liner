namespace MauiApp1;

public partial class App : Application
{
    public static MainPage? MainPageInstance { get; private set; }
    public static SettingsPage? SettingsPageInstance { get; private set; }
    public static DataPage? DataPageInstance { get; private set; }
    public static CommunicationsPage? CommunicationPageInstance { get; private set; }

    public App()
    {
        InitializeComponent();

        // Create singleton instances of all pages
        MainPageInstance = new MainPage();
        SettingsPageInstance = new SettingsPage();
        DataPageInstance = new DataPage();
        CommunicationPageInstance = new CommunicationsPage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        // Handle window closing
        window.Destroying += (s, e) =>
        {
            // Clean up the communication page before closing
            CommunicationPageInstance?.CleanupOnExit();
        };

        return window;

    }
}