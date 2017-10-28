namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class SecurityAnswer
    {
        [SvcBusSerializable]
        public int Id { get; set; }
        [SvcBusSerializable]
        public string Username { get; set; }
        [SvcBusSerializable]
        public int QuestionId { get; set; }
        [SvcBusSerializable]
        public string Answer { get; set; }
        [SvcBusSerializable]
        public string Schema { get; set; }
        [SvcBusSerializable]
        public string TablePrefix { get; set; }

        public bool IsHashed { get; set; }

        public SecurityAnswer()
        {
        }

        public SecurityAnswer(string username, int questionId, string answer)
        {
            Id = 0;
            Username = username;
            QuestionId = questionId;
            Answer = answer;
            IsHashed = true;
        }

        public SecurityAnswer(string username, int questionId, string answer, string schema)
            : this(username, questionId, answer)
        {
            Schema = schema;
            TablePrefix = string.Empty;
            IsHashed = false;
        }

        public SecurityAnswer(int id, int questionId, string answer)
        {
            Id = id;
            Username = string.Empty;
            QuestionId = questionId;
            Answer = answer;
            IsHashed = true;
        }

        public SecurityAnswer(int id, int questionId, string answer, string schema, string tablePrefix)
            : this(id, questionId, answer)
        {
            Schema = schema;
            TablePrefix = tablePrefix;
            IsHashed = false;
        }
    }
}

