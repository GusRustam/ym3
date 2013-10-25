using System;
using System.Windows.Forms;
using Connect;
using ContainerAgent;
using DataProvider.DataLoaders;
using DataProvider.DataLoaders.Status;
using DataProvider.RawData;
using ThomsonReuters.Interop.RTX;

namespace EikonConnectWF {
    public partial class MainForm : Form {
        private IConnection _connection;
        private ISubscription _subscription;
        private AdxRtList _adxRtList;

        public MainForm() {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e) {
            if (_connection == null) {
                _connection = Agent.Factory().GetInstance<IConnection>();
                _connection.Disconnected += () => this.GuiAsync(() => {
                    StatusTextBox.Text = "Disconnected";
                    ConnectButton.Enabled = true;
                    DisconnectButton.Enabled = false;
                    TryAdfinButton.Enabled = true;
                    KillRequestButton.Enabled = false;
                });
                _connection.Connected += () => this.GuiAsync(() => {
                    StatusTextBox.Text = "Connected";
                    ConnectButton.Enabled = false;
                    DisconnectButton.Enabled = true;
                    TryAdfinButton.Enabled = true;
                    KillRequestButton.Enabled = false;
                });
                _connection.ConnectionTimeout += () => this.GuiAsync(() => {
                    StatusTextBox.Text = "ConnectionTimeout";
                    ConnectButton.Enabled = true;
                    DisconnectButton.Enabled = false;
                    TryAdfinButton.Enabled = true;
                    KillRequestButton.Enabled = false;
                });
                _connection.LocalMode += () => this.GuiAsync(() => {
                    StatusTextBox.Text = "LocalMode";
                    ConnectButton.Enabled = false;
                    DisconnectButton.Enabled = true;
                    TryAdfinButton.Enabled = true;
                    KillRequestButton.Enabled = false;
                });
                _connection.Offline += () => this.GuiAsync(() => {
                    StatusTextBox.Text = "Offline";
                    ConnectButton.Enabled = true;
                    DisconnectButton.Enabled = false;
                    TryAdfinButton.Enabled = true;
                    KillRequestButton.Enabled = false;
                });
            }
            _connection.Connect(TimeSpan.FromSeconds(10));
        }

        private void DisconnectButton_Click(object sender, EventArgs e) {
            _connection.Disconnect();
        }

        private void Form1_Load(object sender, EventArgs e) {
            StatusTextBox.Text = "Started";
        }

        private void TryAdfinButton_Click(object sender, EventArgs e) {
            DataStatusTextBox.Text = "";
            try {
                var realtime = Agent.Factory().GetInstance<IRealtime>();

                var subscriptionSetup = realtime.CreateSubscribtion("GAZP.MM");
                subscriptionSetup = subscriptionSetup.WithFeed("IDN");
                subscriptionSetup = subscriptionSetup.WithFields("BID", "ASK");
                subscriptionSetup = subscriptionSetup.WithFrq(TimeSpan.FromSeconds(5));

                subscriptionSetup.OnTime(OnAdxRtListOnOnTime);
                subscriptionSetup.OnDataImage(OnAdxRtListOnOnImage);
                subscriptionSetup.OnDataUpdated(OnAdxRtListOnOnUpdate);
                subscriptionSetup.OnStatusUpdated(OnAdxRtListOnOnStatusChange);

                subscriptionSetup.Create();

                _subscription = subscriptionSetup.Create();

                //_subscription.RequestImage();
                _subscription.QueryOnTime();

                TryAdfinButton.Enabled = false;
                KillRequestButton.Enabled = true;

                this.GuiAsync(() => {
                    DataStatusTextBox.Text += "Subscription created\r\n";
                });
            } catch (Exception) {
                this.GuiAsync(() => {
                    DataStatusTextBox.Text = "Failed to create Subscription\r\n";
                });
            }
        }

        private void KillRequestButton_Click(object sender, EventArgs e) {
            if (_subscription == null) return;
            _subscription.StopUpdates();
            _subscription.Close();
            _subscription = null;
            TryAdfinButton.Enabled = true;
            KillRequestButton.Enabled = false;
        }

        private void RequestDataButton2_Click(object sender, EventArgs e) {
            DataStatusTextBox.Text = "";
            try {
                _adxRtList = Agent.Factory().GetInstance<IEikonObjects>().CreateAdxRtList();
                _adxRtList.DebugLevel = RT_DebugLevel.RT_DEBUG_IMMEDIATE;
                _adxRtList.ErrorMode =AdxErrorMode.EXCEPTION;
                _adxRtList.Source = "IDN";
                _adxRtList.OnStatusChange += OnAdxRtListOnOnStatusChange;
            } catch (Exception exception) {
                _adxRtList = null;
                this.GuiAsync(() => { DataStatusTextBox.Text = exception.ToString(); });
            }
        }

        private void KillRequestButton2_Click(object sender, EventArgs e) {
            if (_adxRtList == null)
                return;
            _adxRtList.UnregisterAllItems();
            _adxRtList.CloseAllLinks();
            _adxRtList = null;
        }

        private void ImageButton_Click(object sender, EventArgs e) {
            DataStatusTextBox.Text += "=================\r\n";
            if (_adxRtList == null)
                return;
            try {
                _adxRtList.UnregisterAllItems();
                _adxRtList.RegisterItems("GAZP.MM", "BID, ASK");

                _adxRtList.Mode = "";
                _adxRtList.OnImage += OnAdxRtListOnOnImage;

                _adxRtList.StartUpdates(RT_RunMode.RT_MODE_IMAGE);
            } catch (Exception exception) {
                _adxRtList = null;
                this.GuiAsync(() => { DataStatusTextBox.Text = exception.ToString(); });
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e) {
            DataStatusTextBox.Text += "=================\r\n";
            if (_adxRtList == null)
                return;
            try {
                _adxRtList.UnregisterAllItems();
                _adxRtList.RegisterItems("GAZP.MM", "BID, ASK");

                _adxRtList.Mode = "";
                _adxRtList.OnUpdate += OnAdxRtListOnOnUpdate;
                
                _adxRtList.StartUpdates(RT_RunMode.RT_MODE_ONUPDATE);
            } catch (Exception exception) {
                _adxRtList = null;
                this.GuiAsync(() => { DataStatusTextBox.Text = exception.ToString(); });
            }
        }

        private void TimeButton_Click(object sender, EventArgs e) {
            DataStatusTextBox.Text += "=================\r\n";
            if (_adxRtList == null)
                return;
            try {
                _adxRtList.UnregisterAllItems();
                _adxRtList.RegisterItems("GAZP.MM", "BID, ASK");

                _adxRtList.Mode = "FRQ:5s";
                _adxRtList.OnTime += OnAdxRtListOnOnTime;

                _adxRtList.StartUpdates(RT_RunMode.RT_MODE_ONTIME);
            } catch (Exception exception) {
                _adxRtList = null;
                this.GuiAsync(() => { DataStatusTextBox.Text = exception.ToString(); });
            }
        }

        private void UpdateOrTimeButton_Click(object sender, EventArgs e) {
            DataStatusTextBox.Text += "=================\r\n";
            if (_adxRtList == null)
                return;
            try {
                _adxRtList.UnregisterAllItems();
                _adxRtList.RegisterItems("GAZP.MM", "BID, ASK");

                _adxRtList.Mode = "FRQ:5s";
                _adxRtList.OnTime += OnAdxRtListOnOnTime;
                _adxRtList.OnUpdate += OnAdxRtListOnOnUpdate;

                _adxRtList.StartUpdates(RT_RunMode.RT_MODE_ONTIME_IF_UPDATED);
            } catch (Exception exception) {
                _adxRtList = null;
                this.GuiAsync(() => { DataStatusTextBox.Text = exception.ToString(); });
            }
        }

        private void OnAdxRtListOnOnUpdate(string name, object tag, RT_ItemStatus status) {
            this.GuiAsync(() => { DataStatusTextBox.Text += string.Format("OnUpdate1({0}, {1}, {2})\r\n", name, tag, status); });
        }

        private void OnAdxRtListOnOnStatusChange(RT_ListStatus status, RT_SourceStatus sourceStatus, RT_RunMode mode) {
            this.GuiAsync(() => { DataStatusTextBox.Text += string.Format("OnStatusChange1({0}, {1}, {2})\r\n", status, sourceStatus, mode); });
        }

        private void OnAdxRtListOnOnImage(RT_DataStatus status) {
            this.GuiAsync(() => { DataStatusTextBox.Text += string.Format("OnImage1({0})\r\n", status); });
        }

        private void OnAdxRtListOnOnTime() {
            this.GuiAsync(() => { DataStatusTextBox.Text += "OnTime1()\r\n"; });
        }


    }

    public static class FormExtenstion {
        public static void GuiAsync(this Form f, Action action) {
            if (f.InvokeRequired) {
                f.Invoke(action);
            } else {
                action();
            }
        }
    }
}
