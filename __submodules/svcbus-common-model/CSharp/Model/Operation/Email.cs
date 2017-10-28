using System;
using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class Email : ICloneable
    {
        public int Id { get; set; }
        [SvcBusSerializable]
        public string EmailFrom { get; set; }
        [SvcBusSerializable]
        public string EmailTo { get; set; }
        [SvcBusSerializable]
        public string EmailBcc { get; set; }
        [SvcBusSerializable]
        public string EmailCc { get; set; }
        public EmailStatus Status { get; set; }
        public DateTime Created { get; set; }
        [SvcBusSerializable]
        public string Subject { get; set; }
        [SvcBusSerializable]
        public string Message { get; set; }
        [SvcBusSerializable]
        public string TemplateName { get; set; }
        
        [SvcBusSerializable]
        public List<NameValuePair> DataMap{ get; set; }

        public string StatusStr { get { return Status.ToString(); } }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public Email()
        {
            DataMap = new List<NameValuePair>();
        }

        public Email(int id, string emailFrom, string emailTo, string status, DateTime created, string subject, string message) : this()
        {
            Id = id;
            EmailFrom = emailFrom;
            EmailTo = emailTo;
            Created = created;
            Subject = subject;
            Message = message;
            Status = (EmailStatus)Enum.Parse(typeof(EmailStatus), status);
        }

        public Email(string emailFrom, string emailTo, EmailStatus status, string subject, string message) : this()
        {
            EmailFrom = emailFrom;
            EmailTo = emailTo;
            Subject = subject;
            Message = message;
            Status = status;
        }

        public Email(int id, string emailFrom, string emailTo, string status, DateTime created, string subject, string message, string emailBcc, string emailCc) 
            : this(id, emailFrom, emailTo, status, created, subject, message)
        {
            EmailBcc = emailBcc;
            EmailCc = emailCc;
        }

        public Email(string emailFrom, string emailTo, EmailStatus status, string subject, string message, string emailBcc, string emailCc)
            : this(emailFrom, emailTo, status, subject, message)
        {
            EmailBcc = emailBcc;
            EmailCc = emailCc;
        }
    }
}
