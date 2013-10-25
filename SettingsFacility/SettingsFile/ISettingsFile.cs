using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace SettingsFacility.SettingsFile {
    /// <summary>
    /// General settings file
    /// </summary>
    public interface ISettingsFile {
        /// <summary>
        /// Indicates whether there's a file on a disk that stores the settings
        /// </summary>
        bool InMemory { get; }

        /// <summary>
        /// Handle of a settings file if any, null unless
        /// </summary>
        FileInfo Storage { get; set; }

        /// <summary>
        /// Loads settings from file. 
        /// </summary>
        /// <remarks>
        /// It raises Updated event on each change in setting being loaded, 
        /// changes UnsavedSettings and UnsavedChanges
        /// </remarks>
        void LoadSettings();

        /// <summary>
        /// Stores all changes into file. Resets UnsavedSettings and UnsavedChanges
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// true if something had changed
        /// </summary>
        bool UnsavedChanges { get; }

        /// <summary>
        /// List of changed settings. Only leaf settings are included
        /// </summary>
        ReadOnlyCollection<ISetting> UnsavedSettings { get; }

        /// <summary>
        /// Freezes and unfreezes Update event. 
        /// </summary>
        /// <remarks>
        /// Even if frozen, UnsavedChanges and UnsavedSettings are still updated
        /// When unfreezed, raises UpdatedBatch event if changed
        /// </remarks>
        bool Frozen { get; set; }

        /// <summary>
        /// Reports change in some setting.
        /// </summary>
        event Action<ISetting> Updated;

        /// <summary>
        /// Reports change in several settings. 
        /// </summary>
        /// <remarks>
        /// Happens on UnFreeze.
        /// UnFreeze is never called inside the object, hence, the only way to get
        /// this event is call <code>Frozen = false</code> externally.
        /// </remarks>
        event Action<List<ISetting>> UpdatedBatch;
    }
}