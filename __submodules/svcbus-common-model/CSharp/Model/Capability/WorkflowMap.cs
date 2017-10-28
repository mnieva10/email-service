using System.Xml.Serialization;

namespace Sovos.SvcBus.Common.Model.Capability
{
    [XmlRoot]
    public class WorkflowMap
    {
        public Workflow[] Workflows { get; set; }
    }

    public class Workflow
    {
        [XmlAttribute]
        public string Name { get; set; }

        public WorkflowStep[] Steps { get; set; }

        public object Clone()
        {
            var obj = new Workflow { Name = Name, Steps = new WorkflowStep[Steps.Length] };
            for (var i = 0; i < Steps.Length; i++)
                obj.Steps[i] = (WorkflowStep)Steps[i].Clone();
            return obj;
        }
    }

    public partial class WorkflowStep
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Optional { get; set; }

        [XmlAttribute]
        public string SettingName { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
