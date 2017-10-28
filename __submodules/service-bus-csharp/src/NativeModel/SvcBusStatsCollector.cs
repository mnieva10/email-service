using System;

namespace Sovos.SvcBus
{
  using SvcBusStatsCollector = IntPtr;

  public class StatsCollector
  {
    public SvcBusStatsCollector Handle { get; protected set; }

    public uint FlushInterval
    {
      set
      {
        NativeMethods.SvcBusStatsCollector_setFlushInterval(Handle, value);
        GC.KeepAlive(this);
      }
    }

    public StatsCollector(SvcBusStatsCollector handle)
    {
      Handle = handle;
    }

    public void SendReport(string statTags, string statValues = "")
    {
      NativeMethods.SvcBusStatsCollector_send_report(Handle, statTags, statValues);
      GC.KeepAlive(this);
    }

    public Oid ReportInit(string statTags, string statValues = "", uint timeout = 0)
    {
      var reportId = Builder.newOid();
      NativeMethods.bson_oid_init(reportId.oid);
      NativeMethods.SvcBusStatsCollector_report_init(Handle, reportId.oid, statTags, statValues, timeout);
      GC.KeepAlive(this);
      return reportId;
    }

    public void ReportCompleteByTimeElapsed(Oid reportId, string statValues = "")
    {
      NativeMethods.SvcBusStatsCollector_report_complete_by_time_elapsed(Handle, reportId.oid, statValues);
      GC.KeepAlive(reportId);
      GC.KeepAlive(this);
    }

    public void ReportCompleteTimeout(Oid reportId, string statValues = "")
    {
      NativeMethods.SvcBusStatsCollector_report_complete_timeout(Handle, reportId.oid, statValues);
      GC.KeepAlive(reportId);
      GC.KeepAlive(this);
    }

    public void CancelReport(Oid reportId)
    {
      NativeMethods.SvcBusStatsCollector_cancel_report(Handle, reportId.oid);
      GC.KeepAlive(reportId);
      GC.KeepAlive(this);
    }

    public bool Enabled
    {
      set
      {
        NativeMethods.SvcBusStatsCollector_setEnabled(Handle, (byte) (value ? 1 : 0));
        GC.KeepAlive(this);
      }
    }
  }
}
