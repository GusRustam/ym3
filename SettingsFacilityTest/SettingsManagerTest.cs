using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using Moq;
using NUnit.Framework;
using SettingsFacility.Setting;
using SettingsFacility.SettingsFile;
using SettingsFacility.Tools;

namespace SettingsFacilityTest {
    public class Manager : SettingsFileBase<Manager.MainGroup>, ISupportsLogging {
        public Manager(ILogger logger, FileInfo file = null) : base(file) {
            Logger = logger;
        }

        public class MainGroup : GroupSetting {
            public IntegerSetting NumInterpolationPoints { get; private set; }

            private readonly ViewportGroup _viewport;
            public ViewportGroup Viewport { get { return _viewport; } }

            // initializing and registering properties
            internal MainGroup(ILogger logger)
                : base(logger, "settings", "settings", "", null) {
                NumInterpolationPoints = new IntegerSetting(
                    "Number of interpolation points",
                    "num-interp-points",
                    "Number of points used to calculate interpolated curve",
                    50);
                _viewport = new ViewportGroup(this, Logger);

                // registering elements
                foreach (var elem in new SettingBase[] { _viewport, NumInterpolationPoints }) {
                    Add(elem);
                }
            }

            public class ViewportGroup : GroupSetting {
                private readonly DurationGroup _modDuration;
                public DurationGroup ModDuration { get { return _modDuration; } }

                private readonly DurationGroup _avgLife;
                public DurationGroup AvgLife { get { return _avgLife; } }

                private readonly DurationGroup _macDuration;
                public DurationGroup MacDuration { get { return _macDuration; } }

                // initializing and registering properties
                internal ViewportGroup(GroupSetting parent, ILogger logger)
                    : base(logger, "Viewport settings", "viewport", "Chart axis parameters", parent) {
                        _modDuration = new DurationGroup(this, "modified duration", "modified-duration", Logger);
                        _avgLife = new DurationGroup(this, "average life", "average-life", Logger);
                        _macDuration = new DurationGroup(this, "Macauley duration", "macauley-duration", Logger);

                    // registering elements
                    foreach (var elem in new SettingBase[] { _modDuration, _avgLife, _macDuration }) {
                        Add(elem);
                    }
                }

                public class DurationGroup : GroupSetting {
                    private readonly DoubleSetting _max;
                    public DoubleSetting Max { get { return _max; } }

                    private readonly DoubleSetting _min;
                    public DoubleSetting Min { get { return _min; } }

                    private readonly BooleanSetting _maxFixed;
                    public BooleanSetting MaxFixed { get { return _maxFixed; } }

                    private readonly BooleanSetting _minFixed;
                    public BooleanSetting MinFixed { get { return _minFixed; } }

                    // initializing and registering properties
                    internal DurationGroup(GroupSetting parent, string humanName, string xmlName, ILogger logger) :
                        base(logger, humanName, xmlName, String.Format("X axis {0} settings", humanName), parent) {
                        _min = new DoubleSetting(
                            String.Format("Minimum {0}", humanName),
                            String.Format("min-{0}", xmlName),
                            String.Format("Minimum {0} visible on the chart", humanName));

                        _max = new DoubleSetting(
                            String.Format("Maximum {0}", humanName),
                            String.Format("max-{0}", xmlName),
                            String.Format("Maximum {0} visible on the chart", humanName));

                        _minFixed = new BooleanSetting(
                            String.Format("Fix minimum {0}", humanName),
                            String.Format("min-{0}-fixed", xmlName),
                            "Fix left side of X axis");

                        _maxFixed = new BooleanSetting(
                            String.Format("Fix maximum {0}", humanName),
                            String.Format("max-{0}-fixed", xmlName),
                            "Fix right side of X axis");

                        // registering elements
                        foreach (var elem in new SettingBase[] { _min, _max, _minFixed, _maxFixed }) {
                            Add(elem);
                        }
                    }
                }
            }
        }

        protected override void InitSettings() {
            Settings = new MainGroup(Logger);
        }

        public ILogger Logger { get; private set; }
    }

    [TestFixture]
    public class SettingsManagerTest : AssertionHelper {
        private readonly ILogger _logger = new Mock<ILogger>().Object;
        public TestContext TestContext { get; set; }

        [TestCase]
        public void ManagerTest() {
            var target = new Manager(_logger);
            Expect(target.Settings.NumInterpolationPoints.Value, Is.EqualTo(target.Settings.NumInterpolationPoints.DefaultValue));
        }

        [TestCase]
        public void DoubleTest() {
            var target = new DoubleSetting("", "", "");

            // testing default value
            Expect(target.Value, Is.EqualTo(0));

            // testing number formats
            Expect(target.Decoder.Encode(12), Is.EqualTo("12"));
            target.SaveFormat = "F4";
            Expect(target.Decoder.Encode(12), Is.EqualTo("12.0000"));
            target.SaveFormat = null;
            Expect(target.Decoder.Encode(12), Is.EqualTo("12"));
            target.SaveFormat = "F2";
            Expect(target.Decoder.Encode(12), Is.EqualTo("12.00"));
            target.SaveFormat = "";
            Expect(target.Decoder.Encode(12), Is.EqualTo("12"));

            // testing invalid numbers
            Expect(target.Decoder.Decode("1eas"), Is.Null);
            Expect(target.Decoder.Decode("1e-2"), Is.EqualTo(0.01d));

            // testing floating point separators
            var currentSeparator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            var invariantSeparator = NumberFormatInfo.InvariantInfo.NumberDecimalSeparator;

            var numberStr = string.Format("12{0}02", currentSeparator);
            Expect(target.Decoder.Decode(numberStr), currentSeparator == invariantSeparator ? Is.EqualTo(12.02) : Is.Not.EqualTo(12.02));

            numberStr = string.Format("12{0}02", invariantSeparator);
            Expect(target.Decoder.Decode(numberStr), Is.EqualTo(12.02));

            // testing nulls
            Expect(target.Decoder.Decode(""), Is.EqualTo(null));
            Expect(target.Decoder.Encode(null), Is.EqualTo(DecoderConstants.NoValue));
        }

        [TestCase]
        public void StringArrTest() {
            var target = new StringArrSetting("", "", "", null);
            var encodedArray = target.Decoder.Encode(new[] { "mama", "papa", "zhopa" });
            Expect(encodedArray, Is.EqualTo("[mama,papa,zhopa]"));

            var decodedArray = target.Decoder.Decode(encodedArray).ToList();
            Expect(decodedArray, Is.Not.Null);
            Expect(decodedArray.Count(), Is.EqualTo(3));
        }

        [TestCase]
        public void DoubleArrTest() {
            var target = new DoubleArrSetting("", "", "", null);
            var encodedArray = target.Decoder.Encode(new double?[] {10, 100.12, 89.44445 });
            Expect(encodedArray, Is.EqualTo("[10,100.12,89.44445]"));

            var decodedArray = target.Decoder.Decode(encodedArray).ToList();
            Expect(decodedArray, Is.Not.Null);
            Expect(decodedArray.Count(), Is.EqualTo(3));

            decodedArray = target.Decoder.Decode("[ 10, 100.12, 89.44445, 02.76, sdhfsdkj, google.com, 12309102u, 1e-3]").ToList();
            Expect(decodedArray, Is.Not.Null);
            Expect(decodedArray.Count(), Is.EqualTo(5));
            Expect(decodedArray[0], Is.EqualTo(10));
            Expect(decodedArray[1], Is.EqualTo(100.12));
            Expect(decodedArray[2], Is.EqualTo(89.44445));
            Expect(decodedArray[3], Is.EqualTo(2.76));
            Expect(decodedArray[4], Is.EqualTo(1e-3));

            decodedArray = target.Decoder.Decode("12312qwdasd, 23, 2345gj0e0rahg 0h23ur- 923 ur=12u trevg").ToList();
            Expect(decodedArray, Is.Empty);
        }

        [TestCase]
        public void ConstructorTest() {
            var manager = new Manager(_logger);
            Expect(manager.InMemory, Is.True);

            var duration = manager.Settings.Viewport.ModDuration;
            duration.Min.Value = 0;
            duration.Max.Value = 10;

            Expect(duration.Min.Value, Is.EqualTo(0));
            Expect(duration.Max.Value, Is.EqualTo(10));

            Expect(duration.MinFixed.Value, Is.False);
            duration.MinFixed.Value = true;
            Expect(duration.MinFixed.Value, Is.True);
            duration.MinFixed.Value = false;
            Expect(duration.MinFixed.Value, Is.False);

            Expect(duration.MaxFixed.Value, Is.False);
            duration.MaxFixed.Value = true;
            Expect(duration.MaxFixed.Value, Is.True);
            duration.MaxFixed.Value = false;
            Expect(duration.MaxFixed.Value, Is.False);
        }

        [TestCase] 
        public void CorrectMemorySave() {
            var manager = new Manager(_logger);
            Expect(manager.Storage, Is.Null);
            manager.SaveSettings();
            Expect(manager.Storage, Is.Null);

            manager.Storage = new FileInfo(Path.GetTempFileName());
            
            Expect(manager.Storage.Exists, Is.True);
            Expect(manager.Storage.Length, Is.EqualTo(0));

            manager.SaveSettings();

            Expect(manager.Storage.Exists, Is.True);
            Expect(manager.Storage.Length, Is.GreaterThan(0));
        }

        [TestCase] 
        public void UnsavedChangesTest() {
            var manager = new Manager(_logger);
            
            var numUpdates = 0;
            manager.Updated += q => { numUpdates++; };

            var numBatch = 0;
            manager.UpdatedBatch += q => { numBatch = q.Count;  }; 

            // checking initial conditions
            Expect(manager.InMemory, Is.True);
            Expect(manager.Storage, Is.Null);
            Expect(manager.UnsavedChanges, Is.False);
            Expect(numUpdates, Is.EqualTo(0));

            var duration = manager.Settings.Viewport.ModDuration;
            duration.Min.Value = 0;
            Expect(manager.UnsavedChanges, Is.False);   // it was 0 already, no changes to be logged

            duration.Min.Value = 1;
            Expect(manager.UnsavedChanges, Is.True);    // now it had to change
            Expect(numUpdates, Is.EqualTo(1));          // and Update event had to come up once

            manager.SaveSettings();
            Expect(manager.UnsavedChanges, Is.False);   // call to SaveSettings has to clean info on updates

            manager.Settings.NumInterpolationPoints.Value = 50;
            Expect(manager.UnsavedChanges, Is.False);   // it was already 50, hence no changes
            Expect(numUpdates, Is.EqualTo(1));          // number of updates fixed too

            manager.Settings.NumInterpolationPoints.Value = 30;
            Expect(manager.UnsavedChanges, Is.True);    // something new
            Expect(manager.UnsavedSettings.Count, Is.EqualTo(1));   // one changed property
            Expect(numUpdates, Is.EqualTo(2));          // and one more update event

            manager.Settings.NumInterpolationPoints.Value = 40; // another update
            Expect(manager.UnsavedSettings.Count, Is.EqualTo(1));   // but this property is already changed, thus number has to stay the same
            Expect(numUpdates, Is.EqualTo(3));                  // but update event had to happen

            duration.Min.Value = 50;
            Expect(manager.UnsavedSettings.Count, Is.EqualTo(2)); // new changed field
            Expect(numUpdates, Is.EqualTo(4));                    // new update event

            duration.Min.Value = 50;
            Expect(manager.UnsavedSettings.Count, Is.EqualTo(2));  // no changes
            Expect(numUpdates, Is.EqualTo(4));                     // and no events

            manager.Frozen = true;                                 // events frozen now
            duration.Min.Value = 60;
            Expect(manager.UnsavedSettings.Count, Is.EqualTo(2));  // value is updated, but this field was already marked as changed, so count is same 
            Expect(numUpdates, Is.EqualTo(4));                     // and no update to come

            duration.Max.Value = 70;                               // we didn't touch it before
            Expect(manager.UnsavedSettings.Count, Is.EqualTo(3));  // change had been registered
            Expect(numUpdates, Is.EqualTo(4));                     // but no update happened

            duration.MaxFixed.Value = true;                        // same situation once again for another field
            Expect(manager.UnsavedSettings.Count, Is.EqualTo(4));
            Expect(numUpdates, Is.EqualTo(4));
            
            manager.Frozen = false;                                 // unfreeze
            Expect(numBatch, Is.EqualTo(4));                        // and watch update to come up

            manager.SaveSettings();                                // save and watch 
            Expect(numUpdates, Is.EqualTo(4));                     
            Expect(manager.UnsavedSettings.Count, Is.EqualTo(0));  
        }

        [TestCase]
        public void SaveLoadTest() {
            var manager = new Manager(_logger, new FileInfo(Path.GetTempFileName()));
            Expect(manager.InMemory, Is.False);
            Expect(manager.Storage.Exists, Is.True);
            Expect(manager.Storage.Length, Is.EqualTo(0));

            Expect(manager.UnsavedChanges, Is.False);
            var duration = manager.Settings.Viewport.ModDuration;
            duration.Min.Value = 0;
            Expect(manager.UnsavedChanges, Is.False);
            duration.Min.Value = 1;
            Expect(manager.UnsavedChanges, Is.True);
            duration.Max.Value = 10;

            manager.SaveSettings();
            Expect(manager.Storage.Exists, Is.True);
            Expect(manager.Storage.Length, Is.GreaterThan(0));

            duration.Max.Value = 30;
            Expect(duration.Max.Value, Is.EqualTo(30));

            manager.LoadSettings();
            Expect(duration.Max.Value, Is.EqualTo(10));
        }

        [TestCase] 
        public void SimpleLoad() {
            var directoryName = Path.GetDirectoryName(Assembly.GetAssembly(typeof(SettingsManagerTest)).CodeBase.Substring(8));
            if (directoryName == null) Assert.Inconclusive("Failed to determine current folder");
            
            var filePath = Path.Combine(directoryName, "settings.xml");
            if (!File.Exists(filePath)) Assert.Inconclusive("settings.xml not found in [{0}]", filePath);
            
            var settingsFile = new FileInfo(filePath);
            var manager = new Manager(_logger, settingsFile);

            var settings = manager.Settings;
            // there must be 3 changed settings
            Expect(settings.NumInterpolationPoints.Value, Is.EqualTo(30));
            Expect(settings.Viewport.ModDuration.MinFixed.Value, Is.True);
            Expect(settings.Viewport.ModDuration.Max.Value, Is.EqualTo(10));

            // but they shouldn't be marked as unsaved
            Expect(manager.UnsavedChanges, Is.False);
        }

        [TestCase] 
        public void LoadAfterInitialization() {
            var manager = new Manager(_logger);
            var settings = manager.Settings;

            // there must be 3 unchanged settings
            Expect(settings.NumInterpolationPoints.Value, Is.EqualTo(50));
            Expect(settings.Viewport.ModDuration.MinFixed.Value, Is.False);
            Expect(settings.Viewport.ModDuration.Max.Value, Is.EqualTo(0));

            var directoryName = Path.GetDirectoryName(Assembly.GetAssembly(typeof(SettingsManagerTest)).CodeBase.Substring(8));
            if (directoryName == null)
                Assert.Inconclusive("Failed to determine current folder");

            var filePath = Path.Combine(directoryName, "settings.xml");
            if (!File.Exists(filePath))
                Assert.Inconclusive("settings.xml not found in [{0}]", filePath);

            var settingsFile = new FileInfo(filePath);

            var totalUpd = 0;
            manager.Updated += q => { totalUpd++; };
            manager.Storage = settingsFile;

            // there must be 3 changed settings
            Expect(settings.NumInterpolationPoints.Value, Is.EqualTo(30));
            Expect(settings.Viewport.ModDuration.MinFixed.Value, Is.True);
            Expect(settings.Viewport.ModDuration.Max.Value, Is.EqualTo(10));

            // and they shouldn't be marked as unsaved.
            Expect(manager.UnsavedChanges, Is.False);
            Expect(totalUpd, Is.EqualTo(3));
        }

        [TestCase] 
        public void SwitchOnAnotherFile() {
            var tempFile1 = new FileInfo(Path.GetTempFileName());
            var tempFile2 = new FileInfo(Path.GetTempFileName());

            var manager = new Manager(_logger);        // in-memory

            var counter = 0;
            manager.Updated += s => { counter++; };

            Expect(manager.Settings.NumInterpolationPoints.Value, Is.EqualTo(50));

            manager.Settings.NumInterpolationPoints.Value = 10; // changing value
            Expect(manager.Settings.NumInterpolationPoints.Value, Is.EqualTo(10));
            Expect(counter, Is.EqualTo(1));

            manager.Storage = tempFile1;   // setting empty file as reciever
            Expect(manager.Settings.NumInterpolationPoints.Value, Is.EqualTo(10)); // nothing's got to change
            Expect(counter, Is.EqualTo(1));

            manager.SaveSettings();             // now file 1 not empty
            tempFile1.Refresh();
            Expect(tempFile1.Length, Is.GreaterThan(0));
            Expect(manager.Settings.NumInterpolationPoints.Value, Is.EqualTo(10)); // and nothing's got to change
            Expect(counter, Is.EqualTo(1));

            manager.Storage = tempFile2;   // another empty file
            Expect(manager.Settings.NumInterpolationPoints.Value, Is.EqualTo(10)); // and nothing's got to change
            Expect(counter, Is.EqualTo(1));

            manager.Settings.NumInterpolationPoints.Value = 20;              // thus changing ourself
            Expect(manager.Settings.NumInterpolationPoints.Value, Is.EqualTo(20)); // and it's changed
            Expect(counter, Is.EqualTo(2));
            
            manager.Storage = tempFile1;                                   // switching to file with 10
            Expect(counter, Is.EqualTo(3));
            Expect(manager.Settings.NumInterpolationPoints.Value, Is.EqualTo(10));    // checking out
            
            tempFile2.Refresh();
            Expect(tempFile2.Length, Is.EqualTo(0)); // nothing had to get stored in file 2
        }

        [TestCase]
        [ExpectedException(typeof (InvalidOperationException))]
        public void ExceptionOnUpdate() {
            var manager = new Manager(_logger);
            manager.Updated += setting => { throw new InvalidOperationException(); };
            manager.Settings.NumInterpolationPoints.Value = 100;
        }

        [TestCase]
        public void ExceptionOnUpdateAndRestore() {
            var manager = new Manager(_logger);
            manager.Updated += setting => {
                throw new InvalidOperationException();
            };
            var numBatchUpdates = 0;
            manager.UpdatedBatch += list => {
                numBatchUpdates++; 
            };

            // todo important! this works only 'coz i have implicit conversion operator
            int? oldVal = manager.Settings.NumInterpolationPoints; 
            var exHandled = false;

            Expect(numBatchUpdates, Is.EqualTo(0));
            var memo = manager.GetState();
            try {
                manager.Settings.NumInterpolationPoints.Value = 100;
            } catch (InvalidOperationException) {
                memo.RestoreState();
                exHandled = true;
            }

            Expect(numBatchUpdates, Is.EqualTo(0));
            Expect(manager.Settings.NumInterpolationPoints.Value, Is.EqualTo(oldVal));
            Expect(exHandled, Is.True);
        }

        [TestCase]
        public void SaveKillLoadSave() {
            var file = new FileInfo(Path.GetTempFileName());
            var manager = new Manager(_logger, file);

            Expect(manager.InMemory, Is.False);
            Expect(manager.Storage.Exists, Is.True);
            Expect(manager.Storage.Length, Is.EqualTo(0));

            Expect(manager.UnsavedChanges, Is.False);
            var duration = manager.Settings.Viewport.ModDuration;
            duration.Min.Value = 1;
            duration.Max.Value = 10;
            Expect(manager.UnsavedChanges, Is.True);

            manager.SaveSettings();
            Expect(manager.Storage.Exists, Is.True);
            Expect(manager.Storage.Length, Is.GreaterThan(0));

            file.Delete();

            var handled = false;
            try {
                manager.LoadSettings();
            } catch (IOException) {
                handled = true;
            }
            Expect(handled, Is.True);

            Expect(manager.UnsavedChanges, Is.False);
            Expect(duration.Max.Value, Is.EqualTo(10));
            duration.Max.Value = 100;
            Expect(manager.UnsavedChanges, Is.True);

            manager.SaveSettings();
            Expect(manager.Storage.Exists, Is.True);
            Expect(manager.Storage.Length, Is.GreaterThan(0));
        }
    }
}