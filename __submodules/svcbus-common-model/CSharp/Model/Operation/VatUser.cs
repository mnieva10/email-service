using System;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class VatUser : User
    {
        public string Salt { get; set; }

        public VatUser(string username, string password, string networkid, string realName, string emailAddress,
            DateTime startingDate, int? expireDays, int? maxInactiveDays, DateTime? logDate, string schema,
            string segHash, string liteUsername, string clientName, AuthenticationType? authType)
            : base(username, password, networkid, realName, emailAddress, startingDate, expireDays, maxInactiveDays, logDate, schema, segHash, liteUsername, clientName, authType)
        {
            Salt = string.Empty;
        }

        public VatUser() { }
    }
}