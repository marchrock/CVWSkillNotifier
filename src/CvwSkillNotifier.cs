using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CvwSkillNotifier.Model;
using CvwSkillNotifier.Properties;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Reactive.Bindings.Extensions;

namespace CvwSkillNotifier
{
    [Export(typeof(IPlugin))]
    [Export(typeof(IRequestNotify))]
    [ExportMetadata("Guid", "0FDACB8C-8AD7-400D-9D0C-037CD86A330E")]
    [ExportMetadata("Title", "CVWSkillNotifier")]
    [ExportMetadata("Description", "艦上機/艦載機の熟練度の通知を行います。")]
    [ExportMetadata("Version", "1.1.0.20150913")]
    [ExportMetadata("Author", "@hgzr")]
    public class CvwSkillNotifier : IPlugin, IRequestNotify
    {
        private List<CvWing> _cvWings;
        private bool IsObservingItemyard { get; set; }
        private IDisposable PortSubscribe { get; set; }

        public event EventHandler<NotifyEventArgs> NotifyRequested;

        public void Initialize()
        {
            PortSubscribe = KanColleClient.Current.Proxy.api_port.TryParse<kcsapi_port>().Subscribe(x => SubscribeSlotItemsAtPort());
            _cvWings = new List<CvWing>();
        }

        public void SubscribeSlotItemsAtPort()
        {
            if (IsObservingItemyard || KanColleClient.Current?.Homeport?.Itemyard == null) return;
            KanColleClient.Current?.Homeport?.Itemyard.ObserveProperty(x => x.SlotItems).Subscribe(x => SkillNotifyCheck(x.Values));
            IsObservingItemyard = true;
            PortSubscribe.Dispose();
        }

        public void SkillNotifyCheck(IEnumerable<SlotItem> slotItemData )
        {
            var maxAdentIds = new List<int>();
            var zeroAdentIds = new List<int>();
            
            foreach (var currentCvWing in slotItemData.Select(slotItem => new CvWing {Id = slotItem.Id, Name = slotItem.Info.Name,
                SkillLevel = slotItem.Adept, PreviousSkillLevel=slotItem.Adept, RowData = slotItem}))
            {
                var cvWingIndex = _cvWings.FindIndex(cvw => cvw.Id == currentCvWing.Id);
                if (cvWingIndex == -1)
                {
                    _cvWings.Add(currentCvWing);
                }
                else
                {
                    _cvWings[cvWingIndex].SkillLevel = currentCvWing.SkillLevel;
                    if (_cvWings[cvWingIndex].PreviousSkillLevel != 7 && _cvWings[cvWingIndex].SkillLevel == 7)
                    {
                        maxAdentIds.Add(cvWingIndex);
                    }
                    if (_cvWings[cvWingIndex].PreviousSkillLevel != 0 && _cvWings[cvWingIndex].SkillLevel == 0)
                    {
                        zeroAdentIds.Add(cvWingIndex);
                    }
                    _cvWings[cvWingIndex].PreviousSkillLevel = _cvWings[cvWingIndex].SkillLevel;
                }
            }

            if (!maxAdentIds.Any() && !zeroAdentIds.Any()) return;

            var notifyBody = "";
            if (maxAdentIds.Count == 1)
            {
                notifyBody += string.Format(Resources.MaxAdentSingleCvWing, _cvWings[maxAdentIds[0]].Name);
            }
            else if( maxAdentIds.Any() )
            {
                notifyBody += string.Format(Resources.MaxAdentMultipleCvWing, maxAdentIds.Count);
            }

            notifyBody += (maxAdentIds.Any() && zeroAdentIds.Any()) ? Environment.NewLine : "";

            if (zeroAdentIds.Count == 1)
            {
                notifyBody += string.Format(Resources.ZeroAdentSingleCvWing, _cvWings[zeroAdentIds[0]].Name);
            }
            else if( zeroAdentIds.Any() )
            {
                notifyBody += string.Format(Resources.ZeroAdentMultipleCvWing, maxAdentIds.Count);
            }

            NotifyRequested?.Invoke(this, new NotifyEventArgs("CVWSkillNotify", "艦載機熟練度通知", notifyBody));
        }
    }
}
