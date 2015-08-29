using System.ComponentModel.Composition;
using Grabacr07.KanColleViewer.Composition;

namespace CvwSkillNotifier
{
    [Export(typeof(IPlugin))]
    [ExportMetadata("Guid", "0FDACB8C-8AD7-400D-9D0C-037CD86A330E")]
    [ExportMetadata("Title", "CVWSkillNotifier")]
    [ExportMetadata("Description", "空母航空隊の熟練度が最大になった際に通知を行います。")]
    [ExportMetadata("Version", "1.0")]
    [ExportMetadata("Author", "@hgzr")]
    public class CvwSkillNotifier : IPlugin
    {
        public void Initialize()
        {
            throw new System.NotImplementedException();
        }
    }
}