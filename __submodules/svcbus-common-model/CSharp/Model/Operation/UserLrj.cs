using System;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class UserLrj
    {
        [SvcBusSerializable]
        public int Id { get; set; }

        [SvcBusSerializable]
        public int JobId { get; set; }
        
        [SvcBusSerializable]
        public string Schema { get; set; }

        [SvcBusSerializable]
        public string Username { get; set; }

        [SvcBusSerializable]
        public string SegHash { get; set; }

        [SvcBusSerializable]
        public DateTime CreatedOn { get; set; }

        public UserLrj() { }

        public UserLrj(int id, int jobId, string schema, string username, string seghash, DateTime createdOn)
        {
            Id = id;
            JobId = jobId;
            Schema = schema;
            Username = username;
            SegHash = seghash;
            CreatedOn = createdOn;
        }
    }
}
