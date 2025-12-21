namespace AgUi.IDE.Desktop
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // TODO: Implement project loading functionality
        // TODO: Integrate Blazor components for IDE
        // TODO: Setup OpenCode process manager

        private void OnLoadProjectClicked(object sender, EventArgs e)
        {
            // TODO: Implement project selection and loading
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Info", "Project loading not yet implemented", "OK");
            });
        }
    }
}
