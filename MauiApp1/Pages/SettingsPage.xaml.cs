namespace MauiApp1;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        // Set default values
        positionVerticalEntry.Text = "1.00";
        spacingEntry.Text = "1.00";
        lineHeadingEntry.Text = "0";
        firstPositionEntry.Text = "0.00";
        firstDepthEntry.Text = "0.70";
        lastPositionEntry.Text = "10.00";
        lastDepthEntry.Text = "0.70";
    }

    private void OnSiteTabClicked(object sender, EventArgs e)
    {
        siteTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
        profilerTabButton.BackgroundColor = Colors.White;
        notesTabButton.BackgroundColor = Colors.White;
        tabContentLabel.Text = "Site tab content";
    }

    private void OnProfilerTabClicked(object sender, EventArgs e)
    {
        siteTabButton.BackgroundColor = Colors.White;
        profilerTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
        notesTabButton.BackgroundColor = Colors.White;
        tabContentLabel.Text = "Profiler tab content";
    }

    private void OnNotesTabClicked(object sender, EventArgs e)
    {
        siteTabButton.BackgroundColor = Colors.White;
        profilerTabButton.BackgroundColor = Colors.White;
        notesTabButton.BackgroundColor = Color.FromArgb("#E0E0E0");
        tabContentLabel.Text = "Notes tab content";
    }
}