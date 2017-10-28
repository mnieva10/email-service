using System;

namespace Sovos.SvcBus
{
  [Serializable]
  class BsonException : Exception
  {
    public BsonException(string msg) : base(msg) {}
  }
}