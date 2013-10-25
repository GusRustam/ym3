using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using LoggingFacility;
using LoggingFacility.LoggingSupport;

namespace SettingsFacility.Setting {
    public class GroupSetting : SettingBase, ISupportsLogging { 
        private readonly ObservableCollection<SettingBase> _children = new ObservableCollection<SettingBase>();

        protected GroupSetting(ILogger logger, string name, string xmlTag, string description, GroupSetting parent = null) :
            base(name, xmlTag, description, parent) {
            Logger = logger;
            _children.CollectionChanged += (sender, args) => {
                if (args.Action != NotifyCollectionChangedAction.Add) return;
                foreach (var setting in args.NewItems.Cast<ISetting>()) {
                    setting.Changed += NotifyChanged;
                }
            };
        }

        public override bool Group { get { return true; } }
        
        public override string ToString() {
            return string.Format("Group {0}", Name);
        }

        public ReadOnlyCollection<ISetting> Children {
            get {
                var res = new List<ISetting>();
                foreach (var kid in _children) {
                    if (kid.Group) {
                        var grp = kid as GroupSetting;
                        if (grp != null) res.AddRange(grp.Children);
                    } else res.Add(kid);
                }
                return new ReadOnlyCollection<ISetting>(res);
            }
        }

        internal override XElement ToXml() {
            var res = new XElement(XmlName, 
                new XAttribute("name", Name),
                new XAttribute("descr", Description));
            foreach (var kid in _children) res.Add(kid.ToXml());
            return res;
        }

        internal override void ReadXml(XElement elem) {
            var xName = elem.Attribute("name").Value;
            if (Name != xName) {
                this.Error(string.Format("Supplied XElement name {0} differs from expected name {1}", xName, Name));
                return;
            }
            foreach (var kid in _children) {
                try {
                    var node = elem.Element(kid.XmlName);
                    if (node == null) {
                        this.Warn(string.Format("Kid {0} is null", kid.XmlName));
                        continue;
                    }
                    kid.ReadXml(node);
                } catch (Exception ex) {
                    this.Error(string.Format("Failed to read kid {0}", kid.XmlName), ex);
                }
            }
        }

        public void Add(SettingBase item) {
            _children.Add(item);
        }

        public ILogger Logger { get; private set; }
    }
}