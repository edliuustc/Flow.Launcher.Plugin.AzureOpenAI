using System.Windows.Controls;

namespace Flow.Launcher.Plugin.AzureOpenAI
{
    public partial class SettingsControl : UserControl
    {
        private readonly Settings _settings;

        public SettingsControl(Settings settings)
        {
            InitializeComponent();
            _settings = settings;
            EndpointTextBox.Text = _settings.Endpoint;
            ApiKeyTextBox.Text = _settings.ApiKey;
            ModelNameTextBox.Text = _settings.ModelName;
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _settings.Endpoint = EndpointTextBox.Text;
            _settings.ApiKey = ApiKeyTextBox.Text;
            _settings.ModelName = ModelNameTextBox.Text;
        }
    }
} 