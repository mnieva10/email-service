using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sovos.SvcBus
{
  [AttributeUsage(AttributeTargets.Property)]
  public class SvcBusSerializableAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Class)]
  public class SvcBusBuildableAttribute : Attribute { }

  public class AnonymousType
  {
    public const int Int = 0;
    public const bool Bool = false;
    public static readonly DateTime DateTime = DateTime.MinValue;
    public const double Double = 0.0;
    public const string String = "";
    public const long Long = 0L;
    public const char Char = '\0';
  }

  internal class TypeNameUtils
  {
    public static bool IsAnonymous(string typeName)
    {
      return typeName.StartsWith("<>f__AnonymousType");
    }
  }

  public enum DictionarySerializationMode
  {
    // Dictionaries with string key can be serialized as bson object(Simple mode) where field name is Dictionary key
    // such bson have simpler structure and it's more clear to understand
    // however mongodb doesn't allow field names that contains . or starts with $
    // use ForceComplex mode to avoid this issue(Dictionaries with string key will be also serialized in Complex mode)
    // Dictionaries with non-string key serialized as array ob objects with key/value subobjects(Complex mode)
    Simple,
    ForceComplex
  }

  public sealed class BuildableSerializableTypeMapper
  {
    private static volatile BuildableSerializableTypeMapper _instance;
    private static readonly object _syncRoot = new Object();
    public readonly Dictionary<string, Type> map;
    public static DictionarySerializationMode dictionarySerializationMode;

    public void Register(string className, Type type)
    {
      lock (((ICollection)map).SyncRoot)
        if (!map.ContainsKey(className))
          map.Add(className, type);
    }

    public void Unregister(string className)
    {
      lock (((ICollection)map).SyncRoot)
        if (map.ContainsKey(className))
          map.Remove(className);
    }

    private BuildableSerializableTypeMapper()
    {
      map = new Dictionary<string, Type>();
    }

    public static BuildableSerializableTypeMapper Mapper
    {
      get
      {
        if (_instance != null)
          return _instance;
        lock (_syncRoot)
          if (_instance == null)
            _instance = new BuildableSerializableTypeMapper();
        return _instance;
      }
    }
  }

  public abstract class BaseBsonSerializer
  {
    private static readonly List<KeyValuePair<Type, Type>> Serializers = new List<KeyValuePair<Type, Type>>()
    {
      new KeyValuePair<Type, Type>(typeof (object), typeof (DefaultBsonSerializer)),
      new KeyValuePair<Type, Type>(typeof (Stream), typeof (StreamBsonSerializer)),
      new KeyValuePair<Type, Type>(typeof (ExpandoObject), typeof (ExpandoObjectBsonSerializer))
    };

    [Obsolete("Method only for tests")]
    public static void Clear()
    {
      Serializers.Clear();
    }

    public static void RegisterSerializer(Type targetType, Type serializerType)
    {

      if (serializerType.GetTypeInfo().IsSubclassOf(typeof(BaseBsonSerializer)))
        Serializers.Add(new KeyValuePair<Type, Type>(targetType, serializerType));
    }

    public static void UnregisterSerializer(Type targetType, Type serializerType)
    {
      Serializers.Remove(new KeyValuePair<Type, Type>(targetType, serializerType));
    }

    public static BaseBsonSerializer CreateSerializer(Type targetType)
    {
      return CreateSerializer(targetType, null);
    }

    internal static BaseBsonSerializer CreateSerializer(Type targetType, params object[] args)
    {
      if (targetType == null)
        throw new BsonSerializerException("targetType is null. Can't create serializer.");
      for (var i = Serializers.Count - 1; i >= 0; i--)
      {
        var currentPair = Serializers[i];

        if (currentPair.Key.IsAssignableFrom(targetType))

          return (BaseBsonSerializer)Activator.CreateInstance(currentPair.Value, args);
      }
      throw new BsonSerializerException(string.Format("Could not find serializer for class {0}", targetType));
    }

    protected static bool SerializePrimitive(Bson b, string name, object obj)
    {
      if (obj == null) return true;
      if (obj.GetType().GetTypeInfo().IsEnum)
        return false;
      switch (Type.GetTypeCode(obj.GetType()))
      {
        case TypeCode.Boolean:
          b.append(name, (bool)obj);
          return true;
        case TypeCode.DateTime:
          b.append(name, (DateTime)obj);
          return true;
        case TypeCode.Double:
          b.append(name, (double)obj);
          return true;
        case TypeCode.Int32:
          b.append(name, (int)obj);
          return true;
        case TypeCode.String:
          b.append(name, (string)obj);
          return true;
        case TypeCode.Int64:
          b.append(name, (long)obj);
          return true;
        case TypeCode.Char:
          b.append(name, obj.ToString());
          return true;
      }
      return false;
    }

    protected static void SerializeArray(Bson b, string name, IList collection)
    {
      Bson sub = b.appendArrayBegin(name);
      foreach (var item in collection)
        SerializeCollectionItem(sub, item, collection.GetType(), name);
      b.appendArrayEnd(sub);
    }

    protected static void SerializeCollectionItem(Bson b, object item, Type collectionType, string name)
    {
      var itemType = item.GetType();
      if (itemType.ImplementsIList())
      {
        SerializeArray(b, name, (IList)item);
        return;
      }
      if (SerializePrimitive(b, name, item)) 
        return;
      if (Type.GetTypeCode(collectionType) == TypeCode.Object)
        SerializeObject(itemType, item, b, name);
    }

    public static void SerializeObject(Type type, object source, Bson target, string name, string typeName = "")
    {
      var serializer = Builder.newBsonSerializer(type);
      serializer.Target = target;
      serializer.Source = source;
      serializer.Serialize(name, typeName);
    }

    protected Bson Prepare(string name)
    {
      var b = Target;
      if (name != string.Empty)
        b = Target.appendDocumentBegin(name);
      return b;
    }

    protected void Unprepare(string name, Bson b)
    {
      if (name != string.Empty)
        Target.appendDocumentEnd(b);
    }

    public object Source { get; set; }
    public Bson Target { get; set; }

    public abstract void Serialize(string name, string typeName = "");
  }

  public class DefaultBsonSerializer : BaseBsonSerializer
  {
    [Obsolete("Don't use this constructor. Instead, use the provide for builder")]
    public DefaultBsonSerializer() { }

    [Obsolete("Don't use this constructor. Instead, use the provide for builder")]
    public DefaultBsonSerializer(object source, ref Bson target)
    {
      Source = source;
      Target = target;
    }

    public override void Serialize(string name, string typeName = "")
    {
      if (Target == null || Source == null)
        return;

      // We will select special serializer for ExpandoObject
      if (Source is ExpandoObject)
      {
        SerializeObject(Source.GetType(), Source, Target, name, typeName);
        return;
      }
      var b = Prepare(name);
      var sourceType = Source.GetType();
      var isAnonymousType = TypeNameUtils.IsAnonymous(sourceType.Name);
      if (!isAnonymousType || typeName != "")
        SerializePrimitive(b, DispatcherConstants.ActualType, typeName == "" ? sourceType.Name : typeName);
      foreach (var property in Source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
#if !NETCORE
.Where(property => isAnonymousType || Attribute.IsDefined(property, typeof(SvcBusSerializableAttribute))))
#else
        .Where(property => isAnonymousType || CustomAttributeExtensions.IsDefined(property, typeof (SvcBusSerializableAttribute))))
#endif
        SerializeProperty(b, property);
      Unprepare(name, b);
    }

    private void SerializeProperty(Bson b, PropertyInfo prop)
    {
      var propValue = prop.GetValue(Source, null);
      if (propValue == null)
        return;
      if (SerializePrimitive(b, prop.Name, propValue))
        return;
      if (propValue.GetType().GetTypeInfo().IsEnum)
      {
        if (propValue.GetType().GetTypeInfo().IsDefined(typeof(FlagsAttribute), false))
          SerializeEnum(b, prop.Name, (Enum)propValue);
        else
          SerializePrimitive(b, prop.Name, (int)propValue);
      }
      else if (propValue.GetType().ImplementsIList())
        SerializeArray(b, prop.Name, (IList)propValue);
      else if (propValue.GetType().ImplementsIDictionary())
        SerializeDictionary(b, prop.Name, (IDictionary)propValue);
      else
      {
        var serializer = CreateSerializer(prop.PropertyType);
        serializer.Target = b;
        serializer.Source = propValue;
        serializer.Serialize(prop.Name);
      }
    }

    private static void SerializeDictionarySimple(Bson b, string name, IDictionary dic)
    {
      var sub = b.appendDocumentBegin(name);
      foreach (string key in dic.Keys)
        SerializeCollectionItem(sub, dic[key], dic.GetType(), key);
      b.appendDocumentEnd(sub);
    }

    private static void SerializeDictionaryComplex(Bson b, string name, IDictionary dic)
    {
      var dicValueType = dic.GetType().GetGenericArguments()[1];
      // serialize with object key
      var sub = b.appendArrayBegin(name);
      foreach (var key in dic.Keys)
      {
        var sub1 = sub.appendDocumentBegin(DispatcherConstants.CollectionKey + DispatcherConstants.CollectionValue);
        var sub2 = sub1.appendDocumentBegin(DispatcherConstants.CollectionKey);
        SerializeCollectionItem(sub2, key, dic.GetType(), dicValueType.Name);
        sub1.appendDocumentEnd(sub2);
        var sub3 = sub1.appendDocumentBegin(DispatcherConstants.CollectionValue);
        SerializeCollectionItem(sub3, dic[key], dic.GetType(), dicValueType.Name);
        sub1.appendDocumentEnd(sub3);
        sub.appendDocumentEnd(sub1);
      }
      b.appendArrayEnd(sub);
    }

    private static void SerializeDictionary(Bson b, string name, IDictionary dic)
    {
      var dicKeyType = dic.GetType().GetGenericArguments()[0];
      if (BuildableSerializableTypeMapper.dictionarySerializationMode == DictionarySerializationMode.Simple &&
          dicKeyType == typeof(string))
        SerializeDictionarySimple(b, name, dic);
      else
        SerializeDictionaryComplex(b, name, dic);
    }

    private static void SerializeEnum(Bson b, string name, Enum e)
    {
      Bson sub = b.appendArrayBegin(name);
      var i = 0;
      foreach (Enum flag in Enum.GetValues(e.GetType()))
      {
        var keysVal = Convert.ToInt64(e);
        var flagVal = Convert.ToInt64(flag);

        if ((keysVal & flagVal) == flagVal)
          sub.append(i++.ToString(CultureInfo.InvariantCulture), flag.ToString());
      }

      b.appendArrayEnd(sub);
    }
  }

  public class StreamBsonSerializer : BaseBsonSerializer
  {
    public StreamBsonSerializer() { }

    public StreamBsonSerializer(object source, ref Bson target)
    {
      Source = source;
      Target = target;
    }

    public override void Serialize(string name, string typeName = "")
    {
      var stream = (Stream)Source;
      var rawData = new byte[stream.Length];
      stream.Seek(0, SeekOrigin.Begin);
      stream.Read(rawData, 0, (int)stream.Length);

      Target.append(name, bson_binary_subtype_t.BSON_BIN_BINARY, rawData);
    }
  }

  public class ExpandoObjectBsonSerializer : BaseBsonSerializer
  {
    public ExpandoObjectBsonSerializer() { }

    public ExpandoObjectBsonSerializer(object source, ref Bson target)
    {
      Source = source;
      Target = target;
    }

    public override void Serialize(string name, string typeName = "")
    {
      var b = Prepare(name);
      if (typeName != "")
        SerializePrimitive(b, DispatcherConstants.ActualType, typeName);
      foreach (var kvp in (IDictionary<string, object>)Source)
        if (!SerializePrimitive(b, kvp.Key, kvp.Value))
          if (kvp.Value.GetType().ImplementsIList())
            SerializeArray(b, kvp.Key, kvp.Value as IList);
          else if (kvp.Value.GetType().ImplementsIDictionary() || kvp.Value.GetType().ImplementsGenericEnumerable())
            // ReSharper disable once RedundantJumpStatement
            continue; // Enums and IDictionary not supported within ExpandoObject
          else SerializeObject(kvp.Value.GetType(), kvp.Value, b, kvp.Key);
      Unprepare(name, b);
    }
  }

  public abstract class BaseBsonDeserializer
  {
    private static readonly List<KeyValuePair<Type, Type>> Deserializers = new List<KeyValuePair<Type, Type>>()
    {
      new KeyValuePair<Type, Type>(typeof (object), typeof (BsonDeserializer)),
      new KeyValuePair<Type, Type>(typeof (Stream), typeof (StreamBsonDeserializer)),
      new KeyValuePair<Type, Type>(typeof (ExpandoObject), typeof (ExpandoObjectBsonDeserializer))
    };

    public static Dictionary<string, Type> SerializableTypesMap { get; private set; }
    public Iterator Source { get; set; }

    protected BaseBsonDeserializer()
    {
      SerializableTypesMap = BuildableSerializableTypeMapper.Mapper.map;
    }

    public static void RegisterDeserializer(Type classType, Type deserializerType)
    {
      if (deserializerType.GetTypeInfo().IsSubclassOf(typeof(BaseBsonDeserializer)))
        Deserializers.Add(new KeyValuePair<Type, Type>(classType, deserializerType));
    }

    public static void UnregisterDeserializer(Type classType, Type deserializerType)
    {
      Deserializers.Remove(new KeyValuePair<Type, Type>(classType, deserializerType));
    }

    public static BaseBsonDeserializer CreateDeserializer(Type classType)
    {
      if (classType == null)
        throw new BsonDeserializerException("classType is null. Can't create deserializer");
      for (var i = Deserializers.Count - 1; i >= 0; i--)
        if (Deserializers[i].Key.IsAssignableFrom(classType))
        {
          var deserializerType = Deserializers[i].Value;
          return (BaseBsonDeserializer)Activator.CreateInstance(deserializerType);
        }
      throw new BsonDeserializerException(string.Format("Could not find deserializer for class {0}", classType));
    }

    protected static bool SetValue(ref object obj, Iterator it)
    {
      switch (it.bsonType)
      {
        case bson_type.BSON_BOOL: obj = (bool)it; break;
        case bson_type.BSON_DATE: obj = (DateTime)it; break;
        case bson_type.BSON_DOUBLE: obj = (double)it; break;
        case bson_type.BSON_INT: obj = (int)it; break;
        case bson_type.BSON_LONG: obj = (long)it; break;
        case bson_type.BSON_STRING:
          var s = (string)it;
          if (obj is char && s != string.Empty)
            obj = s[0];
          else
            obj = s;
          break;
        default:
          return false;
      }
      return true;
    }

    protected static Type FindActualObjectType(Type sourceType, Iterator iterator)
    {
      if (iterator.key != DispatcherConstants.ActualType)
        return sourceType;
      var typeName = (string)iterator;
      if (sourceType == null || typeName != sourceType.Name)
        return SerializableTypesMap.ContainsKey(typeName) ? SerializableTypesMap[typeName] : sourceType;
      return sourceType;
    }

    protected static void DeserializeObject(Type type, Iterator iterator, ref object target)
    {
      var deserializer = CreateDeserializer(type);
      deserializer.Source = iterator;
      deserializer.Deserialize(ref target);
    }

    public abstract void Deserialize(ref object target, object context = null);
  }

  internal class PropertyFieldPair
  {
    public PropertyInfo propertyInfo;
    public FieldInfo fieldInfo;
  }

  internal class PropInfoDictionary : Dictionary<string, PropertyFieldPair> { }

  public class BsonDeserializer : BaseBsonDeserializer
  {
    private static readonly ConcurrentDictionary<Type, PropInfoDictionary> _propInfos = new ConcurrentDictionary<Type, PropInfoDictionary>();

    public BsonDeserializer() { }

    public BsonDeserializer(Iterator source)
    {
      Source = source;
    }

    public static Bson CheckSvcBusException(Bson msg)
    {
      var it = msg.find(DispatcherConstants.Exception);
      if (it == null || it.bsonType != bson_type.BSON_STRING)
        return msg;
      var originalExceptionClassName = (string)it;
      var type = Type.GetType(string.Format("Sovos.SvcBus.{0}", originalExceptionClassName));

      it = msg.find(DispatcherConstants.ExceptionMessage);
      var exceptionMessage = (string)it;

      it = msg.find(DispatcherConstants.StackInfo);
      if (it != null && it.bsonType == bson_type.BSON_STRING)
      {
        exceptionMessage += ". Stack information: " + (string)it;
      }

      if (type == typeof(MethodNotFoundException))
        throw (MethodNotFoundException)Activator.CreateInstance(type, exceptionMessage);
      if (type == null || !type.GetTypeInfo().IsSubclassOf(typeof(SvcBusException)))
        throw new DispatcherException(exceptionMessage, originalExceptionClassName);
      it = msg.find(DispatcherConstants.ExceptionSourceMessage);
      var srcMessage = (string)it;
      it = msg.find(DispatcherConstants.ExceptionErrorCode);
      var code = (int)it;
      throw (SvcBusException)Activator.CreateInstance(type, exceptionMessage, code, srcMessage);
    }

    public override void Deserialize(ref object target, object context = null)
    {
      if (Source == null) return;

      DeserializeIterator(ref target, context);
    }

    private static void SetPropertyValue(PropertyFieldPair prop, object target, object value)
    {
      if (prop.fieldInfo == null)
        prop.propertyInfo.SetValue(target, value, null);
      else prop.fieldInfo.SetValue(target, value);
    }

    private void DeserializeIterator(ref object target, object context)
    {
      var firstDeserializationLoop = true;
      PropInfoDictionary propInfoDictionary = null;
      while (Source.next())
      {
        if (firstDeserializationLoop)
        {
          InitTarget(ref target, out propInfoDictionary, context);
          if (target == null)
            return;
          if (target is ExpandoObject)
          {
            DeserializeObject(target.GetType(), Source, ref target);
            return;
          }
          if (propInfoDictionary == null)
            throw new BsonDeserializerException(DispatcherConstants.CouldNotFindPropInfoForClass);
          firstDeserializationLoop = false;
          if (Source.key == DispatcherConstants.ActualType)
            continue;
        }

        var key = Source.key;
        if (!propInfoDictionary.ContainsKey(key))
          continue;
        var prop = propInfoDictionary[key];

        var propertyType = prop.propertyInfo != null ? prop.propertyInfo.PropertyType : prop.fieldInfo.FieldType;
        var propValue = prop.propertyInfo != null ? prop.propertyInfo.GetValue(target, null) : prop.fieldInfo.GetValue(target);
        // Init empty array or dictionary for enumerable type properties
        if (propValue == null && (propertyType.ImplementsGenericEnumerable() || propertyType.ImplementsIDictionary()))
          propValue = Activator.CreateInstance(propertyType);

        switch (Source.bsonType)
        {
          case bson_type.BSON_BOOL:
          case bson_type.BSON_DATE:
          case bson_type.BSON_DOUBLE:
          case bson_type.BSON_INT:
          case bson_type.BSON_LONG:
          case bson_type.BSON_STRING:
            SetValue(ref propValue, Source);
            SetPropertyValue(prop, target, propValue);
            break;

          case bson_type.BSON_ARRAY:
            if (propertyType.GetTypeInfo().IsEnum && propertyType.GetTypeInfo().IsDefined(typeof(FlagsAttribute), false))
              SetPropertyValue(prop, target, DeserializeEnum(propertyType));
            else if (propertyType.ImplementsIDictionary())
            {
              var dic = (IDictionary)propValue;
              DeserializeDictionaryComplex(ref dic, Source);
              SetPropertyValue(prop, target, propValue);
            }
            else if (propValue != null)
            {
              DeserializeArray(ref propValue, Source);
              SetPropertyValue(prop, target, propValue);
            }
            break;

          case bson_type.BSON_BINDATA:
            DeserializeObject(propertyType, Source, ref propValue);
            SetPropertyValue(prop, target, propValue);
            break;

          case bson_type.BSON_OBJECT:
            if (propertyType.ImplementsIDictionary())
            {
              var dic = (IDictionary)propValue;
              DeserializeDictionarySimple(ref dic, Source);
            }
            else
            {
              var subit = Builder.newIterator(Source);
              DeserializeObject(propertyType, subit, ref propValue);
            }
            SetPropertyValue(prop, target, propValue);
            break;
        }
      }
    }

    private static void BuildPropertiesList(Type targetType, PropInfoDictionary propInfoDictionary)
    {
      var isAnonymousType = TypeNameUtils.IsAnonymous(targetType.Name);
      var fieldIndex = 0;
      // For anonymous types, there's a field matching each property. Fields are stored in the same order as
      // the properties. We need them to deserialize the object. This internal fields are non-public.
      var fields = isAnonymousType ? targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance) : null;
      foreach (var property in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
#if !NETCORE
.Where(property => isAnonymousType || Attribute.IsDefined(property, typeof(SvcBusSerializableAttribute))))
#else
        .Where(property => isAnonymousType || CustomAttributeExtensions.IsDefined(property, typeof (SvcBusSerializableAttribute))))
#endif
        propInfoDictionary[property.Name] = new PropertyFieldPair()
        {
          propertyInfo = property,
          // fields are returned in order matching the properties order of the anonymous class
          fieldInfo = fields != null ? fields[fieldIndex++] : null
        };
    }

    private void InitTarget(ref object target, out PropInfoDictionary propInfoDictionary, object context = null)
    {
      if (Source.key == DispatcherConstants.ActualType)
      {
        //CreateInstance of actual type
        var targetType = target != null ? target.GetType() : null;
        var type = FindActualObjectType(targetType, Source);
        if ((target == null || (targetType != null && targetType.Name == TypeCode.Object.ToString())) && targetType != type)
          target = context != null ? Activator.CreateInstance(type, context) : Activator.CreateInstance(type);
      }

      if (target == null)
      {
        target = new ExpandoObject();
        propInfoDictionary = null;
        return;
      }
      if (_propInfos.TryGetValue(target.GetType(), out propInfoDictionary))
        return;
      propInfoDictionary = new PropInfoDictionary();
      BuildPropertiesList(target.GetType(), propInfoDictionary);
      _propInfos.TryAdd(target.GetType(), propInfoDictionary);
    }

    private int DeserializeEnum(Type type)
    {
      var res = 0;
      var subit = Builder.newIterator(Source);
      while (subit.next())
        res |= (int)Enum.Parse(type, (string)subit);
      return res;
    }

    private static object CreateTypeInstance(Type type)
    {
      switch (Type.GetTypeCode(type))
      {
        case TypeCode.String:
          return string.Empty;
        default:
          return !type.GetTypeInfo().IsAbstract ? Activator.CreateInstance(type) : null;
      }
    }

    private static void DeserializeCollectionItem(ref object target, Iterator iterator, Type objectType)
    {
      if (SetValue(ref target, iterator)) return;

      if (iterator.bsonType == bson_type.BSON_ARRAY)
        DeserializeArray(ref target, iterator);
      else
      {
        var subitKey = Builder.newIterator(iterator);
        DeserializeObject(objectType, subitKey, ref target);
      }
    }

    private static void DeserializeDictionarySimple(ref IDictionary target, Iterator iterator)
    {
      var dicKeyType = target.GetType().GetGenericArguments()[0];
      var dicValueType = target.GetType().GetGenericArguments()[1];

      if (dicKeyType != typeof(string))
        return;

      var it = Builder.newIterator(iterator);
      while (it.next())
      {
        var dicValue = CreateTypeInstance(dicValueType);
        // create dictionary with string key from bson object
        DeserializeCollectionItem(ref dicValue, it, dicValueType);
        target.Add(it.key, dicValue);
      }
    }

    private static void DeserializeDictionaryComplex(ref IDictionary target, Iterator iterator)
    {
      var dicKeyType = target.GetType().GetGenericArguments()[0];
      var dicValueType = target.GetType().GetGenericArguments()[1];
      var it = Builder.newIterator(iterator);
      while (it.next()) // iterate through object or array
      {
        var dicValue = CreateTypeInstance(dicValueType);
        var dicKey = CreateTypeInstance(dicKeyType);
        // create dictionary with object key from bson array
        var subsubit = Builder.newIterator(it);
        subsubit.next();
        var itKey = Builder.newIterator(subsubit);
        itKey.next();
        DeserializeCollectionItem(ref dicKey, itKey, dicKeyType);
        subsubit.next();
        var itValue = Builder.newIterator(subsubit);
        itValue.next();
        DeserializeCollectionItem(ref dicValue, itValue, dicValueType);
        target.Add(dicKey, dicValue);
      }
    }

    private static void DeserializeArray(ref object arr, Iterator it)
    {
      var list = (IList)arr;

      var subit = Builder.newIterator(it);
      while (subit.next())
      {
        var myGenericType = arr.GetType().GetGenericArguments()[0]; //Get the type T of IList<T>.
        var targetValue = CreateTypeInstance(myGenericType);

        DeserializeCollectionItem(ref targetValue, subit, myGenericType);

        list.Add(targetValue);
      }
    }
  }

  public class StreamBsonDeserializer : BaseBsonDeserializer
  {
    public override void Deserialize(ref object target, object context = null)
    {
      var data = (byte[])Source;
      var stream = (Stream)target;
      stream.Write(data, 0, data.Length);
      stream.Seek(0, SeekOrigin.Begin);
    }
  }

  public class ExpandoObjectBsonDeserializer : BaseBsonDeserializer
  {
    private static void BuildGenericList(Type elementType, out object arr)
    {
      var listType = typeof(List<>);
      var constructedListType = listType.MakeGenericType(elementType);
      arr = Activator.CreateInstance(constructedListType);
    }

    private static void DeserializeArrayPrimitive(Iterator arrayIt, ref object arr)
    {
      object value = null;
      SetValue(ref value, arrayIt);
      if (arr == null)
        BuildGenericList(value.GetType(), out arr);
      ((IList)arr).Add(value);
    }

    private static Type ObjectType(Iterator it)
    {
      return it.key == DispatcherConstants.ActualType
        ? (FindActualObjectType(null, it) ?? typeof(ExpandoObject))
        : typeof(ExpandoObject);
    }

    private void DeserializeArray(ref object target)
    {
      var arrayIt = Builder.newIterator(Source);
      Type elementType = null;
      object arr = null;
      while (arrayIt.next())
      {
        switch (arrayIt.bsonType)
        {
          case bson_type.BSON_BOOL:
          case bson_type.BSON_DATE:
          case bson_type.BSON_DOUBLE:
          case bson_type.BSON_INT:
          case bson_type.BSON_LONG:
          case bson_type.BSON_STRING:
            {
              DeserializeArrayPrimitive(arrayIt, ref arr);
              break;
            }
          case bson_type.BSON_OBJECT:
            {
              var subObject = Builder.newIterator(arrayIt);
              subObject.next();
              if (elementType == null)
              {
                elementType = ObjectType(subObject);
                BuildGenericList(elementType, out arr);
              }
              var element = Activator.CreateInstance(elementType);
              DeserializeObject(elementType, subObject, ref element);
              ((IList)arr).Add(element);
              break;
            }
        }
        ((IDictionary<string, object>)target)[Source.key] = arr;
      }
    }

    private void DeserializePrimitive(ref object target)
    {
      object propValue = null;
      SetValue(ref propValue, Source);
      ((IDictionary<string, object>)target)[Source.key] = propValue;
    }

    public override void Deserialize(ref object target, object context = null)
    {
      if (Source == null) return;
      do
      {
        switch (Source.bsonType)
        {
          case bson_type.BSON_BOOL:
          case bson_type.BSON_DATE:
          case bson_type.BSON_DOUBLE:
          case bson_type.BSON_INT:
          case bson_type.BSON_LONG:
          case bson_type.BSON_STRING:
            DeserializePrimitive(ref target);
            break;
          case bson_type.BSON_ARRAY:
            DeserializeArray(ref target);
            break;
          case bson_type.BSON_OBJECT:
            var subObject = Builder.newIterator(Source);
            subObject.next();
            var subObjectType = ObjectType(subObject);
            var obj = Activator.CreateInstance(subObjectType);
            DeserializeObject(subObjectType, subObject, ref obj);
            ((IDictionary<string, object>)target)[Source.key] = obj;
            break;
        }
      } while (Source.next());
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
  public class BsonSerializerException : Exception
  {
    public BsonSerializerException(string message) : base(message) { }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
  public class BsonDeserializerException : Exception
  {
    public BsonDeserializerException(string message) : base(message) { }
  }
}