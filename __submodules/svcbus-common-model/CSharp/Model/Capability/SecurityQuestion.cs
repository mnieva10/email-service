namespace Sovos.SvcBus.Common.Model.Capability
{
    [SvcBusBuildable]
    public class SecurityQuestion
    {
        [SvcBusSerializable]
        public int Id { get; set; }
        [SvcBusSerializable]
        public string Question { get; set; }

        public SecurityQuestion() {}

        public SecurityQuestion(int id, string question)
        {
            Id = id;
            Question = question;
        }
    }
}
