using System;
using System.Reflection;

namespace Sovos.SvcBus
{
  public class DispatchInterface
  {
    protected object userData;

    protected virtual bool SupportsMethodDescribe()
    {
      return false;
    }

    protected virtual void DescribeMethod(string methodName, Bson response)
    {
      Bson sub;
      switch (methodName)
      {
        case DispatcherConstants.Describe:
          sub = response.appendArrayBegin(DispatcherConstants.Params);
          response.appendArrayEnd(sub);
          sub = response.appendDocumentBegin(DispatcherConstants.Result);
          sub.append("commands[]", "bsonARRAY: array with supported commands by the service");
          sub.append("commands[]-><command>{}", "bsonOBJECT: object with information about the command");
          sub.append("commands[]-><command>{}->description", "bsonSTRING: description of the command");
          sub.append("commands[]-><command>{}->parameters[]", "bsonARRAY: array with supported parameters by the command");
          sub.append("commands[]-><command>{}->parameters[]-><parameter>{}", "bsonOBJECT: object with information about the parameter");
          sub.append("commands[]-><command>{}->parameters[]-><parameter>{}->name", "bsonSTRING: name of the parameter");
          sub.append("commands[]-><command>{}->parameters[]-><parameter>{}->type", "bsonSTRING: bson type of the parameter");
          response.appendDocumentEnd(sub);
          sub = response.appendArrayBegin(DispatcherConstants.Throws);
          response.appendArrayEnd(sub);
          break;
        case DispatcherConstants.Version:
          response.append(DispatcherConstants.Description, DispatcherConstants.ReturnsTheInterfaceVersionNumber);
          sub = response.appendDocumentBegin(DispatcherConstants.Params);
          response.appendDocumentEnd(sub);
          sub = response.appendDocumentBegin(DispatcherConstants.Result);
          sub.append("version", "bsonINT: version number of the interface. zero (0) if versioning not implemented");
          response.appendDocumentEnd(sub);
          sub = response.appendArrayBegin(DispatcherConstants.Throws);
          response.appendArrayEnd(sub);
          break;
      }
      GC.KeepAlive(response);
    }

    public static void DeserializeRequest(Message msg, ref object obj)
    {
      DeserializeRequest(msg, obj);
    }

    public static object DeserializeRequest(Message msg, object obj = null)
    {
      var result = Deserialize(DispatcherConstants.Params, msg.bson, obj);
      GC.KeepAlive(msg);
      return result;
    }

    public static void DeserializeResponse(Message msg, ref object obj)
    {
      DeserializeResponse(msg, obj);
    }

    public static void DeserializeResponse(Bson msg, ref object obj)
    {
      DeserializeResponse(msg, obj);
    }

    public static object DeserializeResponse(Message msg, object obj = null)
    {
      var result = Deserialize(DispatcherConstants.Response, msg.bson, obj);
      GC.KeepAlive(msg);
      return result;
    }

    public static object DeserializeResponse(Bson msg, object obj = null)
    {
      var result = Deserialize(DispatcherConstants.Response, msg, obj);
      GC.KeepAlive(msg);
      return result;
    }

    private static object Deserialize(string attributeName, Bson msg, object obj)
    {
        var it = msg.find(attributeName);
        if(it == null)
            throw new Exception(string.Format("Attribute {0} not found on Bson object", attributeName));
        var subit = Builder.newIterator(it);
        var deserializer = BaseBsonDeserializer.CreateDeserializer(obj == null ? typeof(object) : obj.GetType());
        deserializer.Source = subit;
        var result = obj;
        deserializer.Deserialize(ref result);
        GC.KeepAlive(msg);
        return result;
    }

    // ReSharper disable once InconsistentNaming
    public DispatchInterface(object userData)
    {
      this.userData = userData;
    }

    public object Describe(Message msg)
    {
      var result = Builder.newMessage(msg.messageId);
      var subObj = result.bson.appendDocumentBegin(DispatcherConstants.Commands);
      foreach (var mi in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
      {
        if (!SupportsMethodDescribe()) continue;
        var sub = subObj.appendDocumentBegin(mi.Name);
        DescribeMethod(mi.Name, sub);
        subObj.appendDocumentEnd(sub);
      }
      result.bson.appendDocumentEnd(subObj);
      GC.KeepAlive(msg);
      return result;
    }

    public virtual object Version(Message msg)
    {
      var res = Builder.newMessage(msg.messageId);
      res.bson.append(DispatcherConstants.Version, 0);
      GC.KeepAlive(msg);
      return res;
    }

    public virtual object Ping(Message msg)
    {
      var result = Builder.newMessage(msg.messageId);
      var sub = result.bson.appendDocumentBegin(DispatcherConstants.Response);
      sub.append("_type", "DataDto");
      sub.append("DataString", "ok");
      sub.append("DataBool", true);
      result.bson.appendDocumentEnd(sub);
      GC.KeepAlive(msg);

      return result;
    }

    public IResponseProcessingStrategy ResponseProcessingStrategy { get; set; }
  }
}