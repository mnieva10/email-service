using System;
using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Operation;

namespace Sovos.SvcBus.Common.Model.Repositories
{
    public interface IAcaXmitRepository
    {
        int Add(AcaXmit acaXmit);
        int Update(AcaXmit acaXmit, AcaXmitLogCodes logCode, string message);
        int UpdateResetStatus(AcaXmit acaXmit);
        int UpdatePreQueuedStatus(AcaXmit acaXmit);

        void AddTransmitInfoToClientEnv(AcaXmit acaXmit, string schema);
        
        AcaXmit Find(AcaXmit acaXmit);
        List<AcaXmit> FindAll();
        List<AcaXmit> FindAllModifiedSince(int numDays);
        List<AcaXmit> FindAllByFilter(AcaXmitFilter acaFilter);
        AcaXmit FindWithLogEntries(AcaXmit acaXmit);
        List<AcaXmit> FindAllBySegHashTaxYearEnvironment(string segHash, int taxYear, string environment);

        List<AcaXmit> FindSubmitCandidates();
        List<AcaXmit> FindSubmitRetryCandidates(int delay);
        List<AcaXmit> FindRetrieveCandidates(int delay);
        List<AcaXmit> FindRetrieveRetryCandidates();
        List<AcaXmit> FindQueueSubmitCandidates();
        List<AcaXmit> FindImportCandidates();

        int Lock(AcaXmit xmit);
        int Unlock(AcaXmit xmit);
        List<AcaXmit> FindUnlockCandidates(int ageInMinutes);

        DateTime? FindCommandResumeDate(string command);
        void SavePause(AcaXmitPause xmitPause);
    }
}
