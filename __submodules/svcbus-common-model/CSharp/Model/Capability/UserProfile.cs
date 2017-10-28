using System;
using Sovos.SvcBus.Common.Model.Extensions;

namespace Sovos.SvcBus.Common.Model.Capability
{
    [SvcBusBuildable]
    public class UserProfile
    {
        [SvcBusSerializable]
        public string Username { get; set; }
        [SvcBusSerializable]
        public string ProfileName { get; set; }
        [SvcBusSerializable]
        public string Type { get; set; }
        [SvcBusSerializable]
        public int Priority { get; set; }
        [SvcBusSerializable]
        public DateTime? ChangedDate { get; set; }
        [SvcBusSerializable]
        public DateTime? AppliedDate { get; set; }
        [SvcBusSerializable]
        public string Deleted { get; set; }
        [SvcBusSerializable]
        public string Schema { get; set; }

        public UserProfile() { }

        public UserProfile(string profileName, string type, int priority)
        {
            ProfileName = profileName;
            Type = type;
            Priority = priority;
        }

        public UserProfile(string username, string profileName, string type, int priority, string schema)
        {
            Username = username.ToUpperNullable().TrimNullable();
            ProfileName = profileName;
            Type = type;
            Priority = priority;
            Schema = schema;
        }

        public UserProfile(string username, string profileName, string type, int priority, DateTime? changedDate, DateTime? appliedDate, string deleted)
            : this(username, profileName, type, priority, string.Empty)
        {
            ChangedDate = changedDate;
            AppliedDate = appliedDate;
            Deleted = deleted;
        }
    }
}
