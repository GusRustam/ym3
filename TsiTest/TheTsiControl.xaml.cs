using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ContainerAgent;
using DataProvider.Loaders.History;
using DataProvider.Loaders.History.Data;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using ThomsonReuters.Container.Integration;

namespace TsiTest {
    public partial class TheTsiControl : IAppServicesProvider, ISupportsLogging {
        public TheTsiControl() {
            InitializeComponent();
        }

        public void SetServicesProvider(IHostServicesProvider servicesProvider) {
        }

        private void CreateAlert_OnClick(object sender, RoutedEventArgs e) {
            MessageBox.Show("Message Box", "Some text", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void CreateNewWindowButton_OnClick(object sender, RoutedEventArgs e) {
            var x = new SomeDialog();
            x.ShowDialog();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            try {
                var factory = Agent.Factory();
                factory.SetDefaultsToProfile(AgentMode.InEikon);

                Logger = factory.GetInstance<ILogger>();
                var history = factory.GetInstance<IHistory>();

                history
                  .AppendField(HistoryField.Bid)
                  .AppendField(HistoryField.Ask)
                  .WithFeed(FeedTextBox.Text)
                  .WithNumRecords(30)
                  .WithHistory(historyContainer => {
                      this.Info("Got it");
                      MessageBox.Show(string.Format("Rics: {0}; Dates: {1}; Fields: {2}",
                          historyContainer.Slice1().Count(),
                          historyContainer.Slice2().Count(),
                          historyContainer.Slice3().Count()));
                  })
                  .Subscribe(RicTextBox.Text)
                  .WithErrorCallback(Callback2)
                  .WithTimeoutCallback(() => this.Info("Timeout!"))
                  .WithTimeout(TimeSpan.FromSeconds(5))
                  .Request();
            } catch (Exception exception) {
                this.Error("Failed", exception);
                MessageBox.Show(exception.ToString());
            }
        }

        //private void Callback1(IHistoryContainer historyContainer) {
        //    this.Info("Callback(ok)");
        //    //MessageBox.Show(historyContainer.Slice1().Count().ToString(CultureInfo.InvariantCulture));
        //}

        private void Callback2(Exception exception) {
            this.Info("Callback(exception)", exception);
            MessageBox.Show(exception.ToString());
        }

        public ILogger Logger { get; private set; }
    }
}
