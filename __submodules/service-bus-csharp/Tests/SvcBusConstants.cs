using System;

namespace SvcBusTests
{
  public static class Constants
  {
    public const string SingleMongoConnStr = "mongodb://dbtestAdmin:password@127.0.0.1/dbtest";
    public const string ReplicaMongoConnStr = "mongodb://dbtestAdmin:password@127.0.0.1:27018,127.0.0.1:27019,127.0.0.1:27020/dbtest?replicaSet=service-bus";
    public static string ConnectionString = SingleMongoConnStr;

    static Constants()
    {
      var envConnStr = Environment.GetEnvironmentVariable("svcbus_cs");
      if (envConnStr != null && envConnStr.StartsWith("mongodb://"))
        ConnectionString = envConnStr;
    }
  }
}
