﻿namespace Sovos.SvcBus.Common.Model.Infrastructure.Logging
{
    public enum LogCodes
    {
        LOG_LOGIN = 0,
        //LOG_LOGINBYNETWORKID = 1,
        //LOG_LOGOFF = 2,
        LOG_LOGINFAILED = 3,
        LOG_ACCOUNTDISABLED = 4,
        LOG_ALIASCHANGED = 5,
        //LOG_RESOURCEEXECUTION = 6,
        LOG_SETTINGSAVED = 7,
        //LOG_SESSIONBOOTED = 8,
        LOG_WHITELISTRESTRICTION = 9,
        LOG_LOGINFAILEDACCOUNTDISABLED = 10,

        LOG_USERPASSWORDSET = 1000,
        //LOG_USERMODIFIED = 1001,
        //LOG_USERADDED = 1002,
        //LOG_USERDELETED = 1003,
        //LOG_USERSUBPROFILESCHANGED = 1004,
        //LOG_USERSUBPROFILESSYNCHRONIZED = 1005,
        //LOG_USERPASSWORDRESETBYANOTHERUSER = 1006,
        LOG_GENTOKEN = 1007,
        LOG_GENTOKENADMIN = 1008,

        LOG_RECORDDELETED = 2000,

        LOG_CNVSTREAM_Removed = 3000,
        LOG_CNVSTREAM_Saved = 3001,
        LOG_CNVSTREAM_Renamed = 3002,
        LOG_CNVSTREAM_DataChanged = 3003,
        LOG_CNVSTREAM_Opened = 3004,

        //LOG_CPOSUCCESS = 6000;
        //LOG_CPOFAILED = 6001;
        //LOG_CPOUNKNOWNERROR = 6002;

        LOG_SFTPFILETRIGGERED = 7000,

        LOG_SECURITYQUESTIONANSWERED = 8000,
        LOG_SECURITYQUESTIONFAILED = 8001,
        LOG_SECURITYQUESTIONASKED = 8002
    }
}
