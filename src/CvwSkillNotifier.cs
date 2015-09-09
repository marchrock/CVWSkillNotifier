using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CvwSkillNotifier.Model;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Reactive.Bindings.Extensions;

namespace CvwSkillNotifier
{
    [Export(typeof(IPlugin))]
    [Export(typeof(IRequestNotify))]
    [Export(typeof(INotifier))]
    [ExportMetadata("Guid", "0FDACB8C-8AD7-400D-9D0C-037CD86A330E")]
    [ExportMetadata("Title", "CVWSkillNotifier")]
    [ExportMetadata("Description", "艦上機/艦載機の熟練度が最大になった際に通知を行います。")]
    [ExportMetadata("Version", "1.0")]
    [ExportMetadata("Author", "@hgzr")]
    public class CvwSkillNotifier : IPlugin, IRequestNotify, INotifier
    {
        private List<CvWing> _cvWings;
        private bool IsObservingItemyard { get; set; }
        
        public event EventHandler<NotifyEventArgs> NotifyRequested;

        public void Initialize()
        {
            KanColleClient.Current.Proxy.api_port.TryParse<kcsapi_port>().Subscribe(x => PortSkillCheck());
            _cvWings = new List<CvWing>();
        }

        public void PortSkillCheck()
        {
            var slotItemData = KanColleClient.Current.Homeport.Itemyard.SlotItems.Values.Where(si => si.Info.IsNumerable);
            SkillNotifyCheck(slotItemData);

            if (IsObservingItemyard || KanColleClient.Current?.Homeport?.Itemyard == null) return;
            KanColleClient.Current?.Homeport?.Itemyard.ObserveProperty(x => x.SlotItems).Subscribe(x => SkillNotifyCheck(x.Values));
            IsObservingItemyard = true;
        }

        public void SkillNotifyCheck(IEnumerable<SlotItem> slotItemData )
        {
            var notifyIds = new List<int>();

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
                        notifyIds.Add(cvWingIndex);
                    }
                    _cvWings[cvWingIndex].PreviousSkillLevel = _cvWings[cvWingIndex].SkillLevel;
                }
            }

            if (!notifyIds.Any()) return;
            string notifyBody;
            if (notifyIds.Count == 1)
            {
                notifyBody = _cvWings[notifyIds[0]].Name + "の熟練度が最大になりました。";
            }
            else
            {
                notifyBody = notifyIds.Count + "機の艦載機の熟練度が最大になりました。";
            }

            NotifyRequested?.Invoke(this, new NotifyEventArgs("CVWSkillNotify", "艦載機熟練度通知", notifyBody));
        }

        public void Notify(INotification notification)
        {
            Notification.Create(notification.Type, notification.Header, notification.Body);
        }
    }
}
