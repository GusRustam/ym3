using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SettingsFacility.Setting;
using Toolbox.Memento;

namespace SettingsFacility.SettingsFile {
    public abstract class SettingsFileBase<T> : ISettingsFile, IOriginator
        where T : GroupSetting {

        public event Action<ISetting> Updated;
        public event Action<List<ISetting>> UpdatedBatch;

        // Container for not saved settings
        private readonly HashSet<ISetting> _unsavedSettings = new HashSet<ISetting>();
        private void ClearUnsaved() {
            _unsavedSettings.Clear();
        }

        // Settings file handle
        private FileInfo _settingsFile;
        public FileInfo Storage {
            get {
                if (_settingsFile != null) _settingsFile.Refresh();
                return _settingsFile;
            }
            set {
                if (_settingsFile == null && value != null && value.Exists && value.Length == 0) {
                    // Присваиваем новый пустой файл, а раньше файла не было
                    foreach (var set in Settings.Children) 
                        _unsavedSettings.Add(set);
                    _settingsFile = value;
                
                } else if (value != null && value.Exists && value.Length > 0) {
                    // неважно был ли другой файл или нет, у меня все сохранено в памяти.
                    // поэтому устанавливаем указатель на новый файл и загружаем его, изменения записываем.
                    _settingsFile = value;
                    LoadSettings();
                    ClearUnsaved(); // изменения нельзя считать несохраненными, они вполне себе сохраненные

                } else if ((_settingsFile != null && _settingsFile != value) || _settingsFile == null) {
                    _settingsFile = value;
                } 
            }
        }

        // Indicates if there's some unsaved changes
        public bool UnsavedChanges { 
            get { return _unsavedSettings.Any(); }
        }

        // Detailed info on unsaved changes
        public ReadOnlyCollection<ISetting> UnsavedSettings {
            get {
                return new ReadOnlyCollection<ISetting>(_unsavedSettings.ToList());
            }
        }

        // Main entrance to settings world
        private T _settings;
        public T Settings {
            get { return _settings; }
            protected set {
                if (_settings != null) throw new InvalidOperationException();
                _settings = value;
                _settings.Changed += s => {
                    _unsavedSettings.Add(s);
                    NotifyUpdated(s);
                };
            }
        }

        private void NotifyUpdatedBatch() {
            if (UpdatedBatch == null || Frozen || !UnsavedChanges) 
                return;

            UpdatedBatch(_unsavedSettings.ToList());
        }

        private void NotifyUpdated(ISetting s) {
            if (Updated == null || Frozen) return;
            if (!UnsavedChanges) return;
                
            Updated(s);
        }

        /// <summary>
        /// Additional level of freezing. For internal access only.
        /// When muted, no event leaves the object. When unmuted also.
        /// </summary>
        private bool Muted { get; set; }

        // Freezeing
        private bool _frozen;

        public bool Frozen {
            get { return Muted || _frozen; }
            set {
                var oldVal = _frozen;
                _frozen = value;

                if (oldVal == value || value || !_unsavedSettings.Any()) return;
                NotifyUpdatedBatch();
            }
        }


        // Indicates whether settings are stored in a file 
        public bool InMemory {
            get { return Storage == null; } 
        }

        // Constructor
        protected SettingsFileBase(FileInfo file = null) {
            _settingsFile = file;
            
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            InitSettings();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor

            if (file != null && file.Exists && file.Length > 0) LoadSettings(); 
            
            // when settings are loaded in constructor, nothing should be marked as changed
            ClearUnsaved();
        }

        protected abstract void InitSettings();

        // Loader
        public void LoadSettings() {
            if (InMemory) return; //|| !Storage.Exists || Storage.Length <= 0

            using (var fileStream = Storage.OpenRead()) {
                var doc = XDocument.Load(fileStream);
                var settings = doc.Element(Settings.XmlName);
                if (settings == null) {
                    throw new InvalidDataException(
                        string.Format("Invalid settings file, no root <{0}> element found", Settings.XmlName));
                }
                Settings.ReadXml(settings);
            }
        }

        // Saver
        public void SaveSettings() {
            try {
                if (InMemory || !UnsavedChanges) return;

                if (!Storage.Exists) {
                    Storage.Create().Dispose();
                    Storage.Refresh();
                }

                using (var fileStream = Storage.OpenWrite()) {
                    var doc = new XDocument(Settings.ToXml());
                    doc.Save(fileStream);
                }
            } finally {
                ClearUnsaved();
            }
        }

        public IMemento GetState() {
            return new SettingsFileMemento(this);
        }

        private class SettingsFileMemento : IMemento {
            private readonly SettingsFileBase<T> _originator;
            private readonly XElement _xml;

            public SettingsFileMemento(SettingsFileBase<T> originator) {
                _originator = originator;
                _xml = originator.Settings.ToXml();
            }

            public void RestoreState() {
                _originator.Muted = true;
                try {
                    _originator.Settings.ReadXml(_xml);
                } finally {
                    _originator.Muted = false;
                }
            }
        }
    }
}