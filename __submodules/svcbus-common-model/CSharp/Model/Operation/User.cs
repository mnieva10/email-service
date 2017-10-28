using System;
using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Extensions;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Operation
{
    public class UserProxy : User
    {
        public UserProxy() { }
    }

    [SvcBusBuildable]
    public class User
    {
        private string _username;
        [SvcBusSerializable]
        public string Username
        {
            get { return _username; }
            set { _username = value.ToUpperNullable().TrimNullable(); }
        }

        private string _password;
        [SvcBusSerializable]
        public string Password
        {
            get { return _password; }
            set { _password = value.TrimNullable(); }
        }

        [SvcBusSerializable]
        public AuthenticationType? AuthenticationType { get; set; }

        [SvcBusSerializable]
        public string NetworkId { get; set; }
        [SvcBusSerializable]
        public string RealName { get; set; }
        [SvcBusSerializable]
        public DateTime StartingDate { get; set; }
        [SvcBusSerializable]
        public DateTime? LogDate { get; set; }
        [SvcBusSerializable]
        public string EmailAddress { get; set; }
        [SvcBusSerializable]
        public int? ExpireDays { get; set; }
        [SvcBusSerializable]
        public int? MaxInactiveDays { get; set; }

        [SvcBusSerializable]
        public string Domain { get; set; }
        [SvcBusSerializable]
        public string Schema { get; set; }
        [SvcBusSerializable]
        public string IPAddress { get; set; }

        [SvcBusSerializable]
        public string SegHash { get; set; }

        private string _liteUsername;
        [SvcBusSerializable]
        public string LiteUsername
        {
            get { return _liteUsername; }
            set { _liteUsername = value.ToUpperNullable().TrimNullable(); }
        }

        [SvcBusSerializable]
        public string ClientName { get; set; }

        [SvcBusSerializable]
        public List<UserProfile> Profiles { get; set; }

        [SvcBusSerializable]
        public List<SecurityAnswer> SecurityAnswers { get; set; }

        [SvcBusSerializable]
        public string Product { get; set; }

        public User()
        {
            Profiles = new List<UserProfile>();
            SecurityAnswers = new List<SecurityAnswer>();
        }

        public User(string username, string password, string networkid, string realName, string emailAddress,
            DateTime startingDate, int? expireDays, int? maxInactiveDays, DateTime? logDate, string schema)
            :this(username, password, networkid, realName, emailAddress, startingDate, expireDays, maxInactiveDays, logDate, schema, null)
        {
        }

        public User(string username, string password, string networkid, string realName, string emailAddress,
            DateTime startingDate, int? expireDays, int? maxInactiveDays, DateTime? logDate, string schema,
            AuthenticationType? authType)
            : this()
        {
            Username = username.ToUpperNullable().TrimNullable();
            Password = password.TrimNullable();
            NetworkId = networkid.ToLowerNullable().TrimNullable();
            RealName = realName.TrimNullable();
            EmailAddress = emailAddress.ToLowerNullable().TrimNullable();
            StartingDate = startingDate;
            ExpireDays = expireDays;
            MaxInactiveDays = maxInactiveDays;
            LogDate = logDate;
            Domain = string.Empty;
            Schema = schema;
            IPAddress = string.Empty;
            SegHash = string.Empty;
            LiteUsername = string.Empty;
            ClientName = string.Empty;
            AuthenticationType = authType;
        }

        public User(string username, string password, string networkid, string realName, string emailAddress,
            DateTime startingDate, int? expireDays, int? maxInactiveDays, DateTime? logDate, string schema, string segHash, string liteUsername, string clientName, AuthenticationType? authType)
            : this(username, password, networkid, realName, emailAddress, startingDate, expireDays, maxInactiveDays, logDate, schema, authType)
        {
            SegHash = segHash;
            LiteUsername = liteUsername;
            ClientName = clientName;
        }

        public User(string username, string password)
            : this(username, password, string.Empty, string.Empty, string.Empty, DateTime.Now, 0, 0, DateTime.Now, string.Empty, null)
        { }

        public User(string username, string password, string domain, string email)
            : this(username, password, string.Empty, string.Empty, string.Empty, DateTime.Now, 0, 0, DateTime.Now, string.Empty, null)
        {
            Domain = domain.ToUpperNullable();
            EmailAddress = email;
        }

        public User(string username, string password, string domain, string email, string segHash, string liteUsername)
            : this(username, password, domain, email)
        {
            SegHash = segHash;
            LiteUsername = liteUsername;
        }
    }
}
