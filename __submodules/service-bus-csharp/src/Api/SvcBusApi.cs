using System;
using System.Runtime.InteropServices;

namespace Sovos.SvcBus
{
  using SvcBusMessage = IntPtr;
  using bson_oid_p = IntPtr;
  using bson = IntPtr;
  using bson_iterator = IntPtr;
  using SvcBusPipeService = IntPtr;
  using SvcBusService = IntPtr;
  using SvcBusConsumer = IntPtr;
  using SvcBusConsumerPool = IntPtr;
  using SvcBusConsumerPoolDictionary = IntPtr;
  using SvcBusProducer = IntPtr;
  using SvcBusResponder = IntPtr;
  using SvcBusGenericErr = IntPtr;
  using SvcBusServicePersistence = IntPtr;
  using SvcBusThreadPool = IntPtr;
  using SvcBusThreadWork = IntPtr;
  using SvcBusThreadPoolTimer = IntPtr;
  using SvcBusMutex = IntPtr;
  using SvcBusStatsCollector = IntPtr;
  using System.Text;
  
  public enum bson_type
  {
    BSON_EOO = 0,
    BSON_DOUBLE = 1,
    BSON_STRING = 2,
    BSON_OBJECT = 3,
    BSON_ARRAY = 4,
    BSON_BINDATA = 5,
    BSON_UNDEFINED = 6, /* Apparently deprecated */
    BSON_OID = 7,
    BSON_BOOL = 8,
    BSON_DATE = 9,
    BSON_NULL = 10,
    BSON_REGEX = 11,
    BSON_DBREF = 12, /**< Deprecated. */
    BSON_CODE = 13,
    BSON_SYMBOL = 14,
    BSON_CODEWSCOPE = 15,
    BSON_INT = 16,
    BSON_TIMESTAMP = 17,
    BSON_LONG = 18,
    BSON_MAXKEY = 127,
    BSON_MINKEY = 255
  };

  public enum svc_bus_responder_err_t
  {
    SERVICE_BUS_RESPONDER_OK = 0,
    SERVICE_BUS_RESPONDER_MONGO_ERROR = 1,       /* need to check errors on mongo object */
    SERVICE_BUS_RESPONDER_PIPE_ERROR = 2         /* used to denote an error trying to bind to a SvcBusPipe object */
  };

  public enum svc_bus_pipeservice_err_t
  {
    SERVICE_BUS_PIPE_SERVICE_OK = 0,
    SERVICE_BUS_PIPE_SERVICE_INVALID_DATABASE_NAME = 1,           /* invalid database name */
    SERVICE_BUS_PIPE_SERVICE_INVALID_WAIT_MODE = 2,               /* [DEPRECATED] pipe mode doesn't match underlaying collection type, see pipe_wait_mode_t */
    SERVICE_BUS_PIPE_SERVICE_MONGO_ERROR = 3,                     /* need to check errors on mongo object */
    SERVICE_BUS_PIPE_SERVICE_ERROR_GETTING_NEXTFUZZ = 4,          /* error attempting to obtain central nextfuzz value */
    SERVICE_BUS_PIPE_SERVICE_INVALID_OPTIONS = 5,                 /* invalid options parameter when binding pipe */
    SERVICE_BUS_PIPE_SERVICE_INVALID_CONNECTION_STRING = 6,       /* invalid connection string */
    SERVICE_BUS_PIPE_SERVICE_AUTH_FAIL = 7,                       /* invalid username or password */
    SERVICE_BUS_PIPE_SERVICE_MONGO_VERSION_IS_NOT_SUPPORTED = 8,  /* service bus can work unstable while using this mongo version */
    SERVICE_BUS_PIPE_SERVICE_PIPE_ERROR = 9
  };

  public enum svc_bus_queue_mode_t
  {
    SERVICE_BUS_VOLATILE = 0,    /* use capped collection and tailable cursor */
    SERVICE_BUS_DURABLE = 1      /* use common collection and polling, see SERVICE_BUS_PIPE_POLLING_INTERVAL */
  };

  [Flags]
  public enum svc_bus_consumer_options_t
  {
    SERVICE_BUS_CONSUMER_OPTION_WAIT_FOR_RESPONSE = (1 << 0),   /* consumer will wait for response if this option is set */
    SERVICE_BUS_CONSUMER_KEEP_RESPONSE_PIPE = (1 << 3)
  };
  
  public enum svc_bus_consumer_err_t
  {
    SERVICE_BUS_CONSUMER_OK = 0,
    SERVICE_BUS_CONSUMER_INVALID_REQUEST_PIPE_NAME = 1,             /* consumer can't pass acquired pipe(response pipe) as request pipe */
    SERVICE_BUS_CONSUMER_RESPONSE_PIPE_CANT_BE_NULL = 2,            /* can't wait on NULL response pipe */
    SERVICE_BUS_CONSUMER_MONGO_ERROR = 3,                           /* need to check errors on mongo object */
    SERVICE_BUS_CONSUMER_WAIT_EXCEEDED = 4,                         /* wait limit exceeded */
    SERVICE_BUS_CONSUMER_FAILED_BINDING_REQUEST_PIPE = 5,           /* failed trying to bind to request pipe */
    SERVICE_BUS_CONSUMER_THREADPOOL_NO_INIT = 6,                    /* pipeservice thread pool not initialized for async operations */
    [Obsolete] 
    SERVICE_BUS_CONSUMER_ASYNC_OP_NOT_PREPARED_FOR_OPERATION = 7,   /* asyncOps object not created with the right options when calling SvcBusConsumer_createAsyncOp() */
    SERVICE_BUS_CONSUMER_FAIL_CREATE_UPDATE_HEARTBEAT_TIMER = 8,    /* failed to create timer that calls updateHeartbeat method on a separate thread */
    SERVICE_BUS_CONSUMER_INVALID_RESPONSE_PIPE_NAME = 9,            /* pipe name is invalid */
    SERVICE_BUS_CONSUMER_REQUEST_PIPE_CANT_BE_NULL = 10,            /* can't send using using NULL request pipe */
    SERVICE_BUS_CONSUMER_ASYNC_DATA_ALREADY_EXISTS = 11,            /* hashed object for given OID passed to async data request already in hashtable */
    SERVICE_BUS_CONSUMER_FAILED_TRUNCATING_RESPONSE_COLLECTION = 12 /* failed trying to truncate response collection */
  };

  public enum svc_bus_producer_err_t
  {
    SERVICE_BUS_PRODUCER_OK = 0,
    SERVICE_BUS_PRODUCER_REQUEST_PIPE_CANT_BE_NULL = 1,         /* failed to bind to request pipe */
    SERVICE_BUS_PRODUCER_WAIT_EXCEEDED = 2,                     /* wait limit exceeded */
    SERVICE_BUS_PRODUCER_MONGO_ERROR = 3,                       /* need to check errors on mongo object */
    SERVICE_BUS_PRODUCER_CREATING_QUERY = 4                     /* this error code denotes some kind of internal error creating some of the cached query objects */ 
  };

  public enum bson_binary_subtype_t
  {
    BSON_BIN_BINARY = 0,
    BSON_BIN_FUNC = 1,
    BSON_BIN_BINARY_OLD = 2,
    BSON_BIN_UUID = 3,
    BSON_BIN_MD5 = 5,
    BSON_BIN_USER = 128
  };
   
  public enum consumer_pool_err
  {
    OK = 0,
    FAILED_CREATING_CONSUMER = 1,
    MANDATORY_OBJECTS_MISSING = 2
  };

  public enum consumer_pool_dictionary_err
  {
    OK = 0,
    FAILED_CREATING_PIPESERVICE = 1,
    FAILED_LOADING_SERVICE = 2,
    FAILED_CREATING_CONSUMER_POOL = 3
  };

  public enum service_err
  {
    OK = 0,
    NULL_SERVICE_NAME = 1
  };

  public enum service_persistence_err
  {
    OK = 0,
    MONGO_ERROR = 1,
    SERVICE_NOT_FOUND = 2
  };

  public enum svc_bus_mutex_operation_result_t
  {
    SERVICE_BUS_MUTEX_OPERATION_SUCCESS = 0,
    SERVICE_BUS_MUTEX_OPERATION_FAILED = 1,
    SERVICE_BUS_MUTEX_OPERATION_ERROR = 2
  }

  public enum svc_bus_mutex_err_t
  {
    SERVICE_BUS_MUTEX_OK = 0,
    SERVICE_BUS_MUTEX_QUERY_LOCKED = 1,
    SERVICE_BUS_MUTEX_ANONYMOUS = 2,
    SERVICE_BUS_MUTEX_MONGO_ERROR = 3
  }

  public static class NativeMethods
  {
    public const int SERVICE_BUS_OK = 0;
    public const int SERVICE_BUS_ERROR = -1;
    public const int SERVICE_BUS_DEFAULT_HEARTBEAT_LIMIT_MS = 60 * 1000 * 5;
    public const string SERVICE_BUS_FIELDNAME_BROADCAST = "brdcst";

    private const string DllVersion = "2-19-1"; /* PLEASE!!! maintain this constant in sync with the dll driver version this code operates with */

    #region DllNameBindingConstants
#if DEBUG
    private const string ConfigType = "d";
#else
    private const string ConfigType = "r";
#endif
#if WIN64
    private const string CpuType = "64";
#else
    private const string CpuType = "32";
#endif
    #endregion

    public const string SvcBusDll = "service-bus_" + ConfigType + CpuType + "_v" + DllVersion + ".dll";

    #region SvcBusGeneralAPI_DLLImports
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bson_oid_p ServiceBusOid_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ServiceBusOid_dealloc(bson_oid_p oid);

     [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void bson_oid_copy(IntPtr src, IntPtr dst);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern void bson_oid_to_string(bson_oid_p oid, [MarshalAs(UnmanagedType.LPStr)] StringBuilder buf);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern void bson_oid_init_from_string(bson_oid_p oid, [MarshalAs(UnmanagedType.LPStr)]string str);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusGenericErr SvcBusGenericErr_t_create();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusGenericErr_t_free(SvcBusGenericErr _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusGenericErr_t_getErrstr(SvcBusGenericErr _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusGenericErr_t_getErr(SvcBusGenericErr _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte SvcBus_isBsonOidEqual(bson_oid_p id1, bson_oid_p id2);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SvcBus_getTickCount();
    
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern void SvcBus_setLogfilePath([MarshalAs(UnmanagedType.LPStr)]string path);
    #endregion

    #region SvcBusBson_DLLImports
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void bson_oid_init(bson_oid_p oid, IntPtr context);

    public static void bson_oid_init(bson_oid_p oid)
    {
      bson_oid_init(oid, IntPtr.Zero);
    }

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bson bson_new();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bson bson_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void bson_free(bson b);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte bson_init(bson b);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void bson_destroy( bson b );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte bson_copy_to(bson src, bson dst);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bson bson_copy(bson src);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr bson_as_json(bson b, int len);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_int32(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, int i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_oid(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, bson_oid_p oid);

    /* Noticed in the following declaration we have declared value as IntPtr. This was done in purpose
     * because C# will not marshal a UTF8 string automatically. To call bson_append_string the UTF8
     * string is created using special API and sent to DLL API as a null terminated IntPtr string.
     * The parameter name is marshaled as Ansi, which is fine. I don't think we need to have UTF8
     * internal field names or BSON member names */
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_utf8(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, IntPtr value, int valueLen);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_double(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, double d);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_int64(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, Int64 i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_bool(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, byte i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_date_time(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, Int64 millis);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_binary(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, int type, IntPtr str, uint len);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_document_begin(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, bson child);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte bson_append_document_end(bson b, bson child);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_document(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, bson doc);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_array_begin(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, bson child);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte bson_append_array_end(bson b, bson child);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_array(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen, bson arr);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_null(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_maxkey(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_append_minkey(bson b, [MarshalAs(UnmanagedType.LPStr)]string name, int nameLen);
    #endregion

    #region SvcBusIterator_DLLImports
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bson_iterator bson_iter_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void bson_iter_dealloc(bson_iterator it);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_iter_init_find(bson_iterator it, bson obj, [MarshalAs(UnmanagedType.LPStr)]string name);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int bson_iter_type(bson_iterator i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void bson_iter_init( bson_iterator i, bson b);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int bson_iter_int32(bson_iterator i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr bson_iter_utf8(bson_iterator i, out uint len);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bson_oid_p bson_iter_oid( bson_iterator i );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern double bson_iter_double(bson_iterator i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte bson_iter_next(bson_iterator i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr bson_iter_key(bson_iterator i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int64 bson_iter_int64(bson_iterator i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte bson_iter_bool(bson_iterator i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int64 bson_iter_date_time(bson_iterator i);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void bson_iter_binary(bson_iterator i, out int subtype, out uint len, out IntPtr data);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte bson_iter_recurse(bson_iterator i, bson_iterator sub);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern byte bson_iter_find_descendant(bson_iterator i, [MarshalAs(UnmanagedType.LPStr)] string dotkey,
      bson_iterator descendant);

    #endregion

    #region SvcBusMessage_DLLImports
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusMessage SvcBusMessage_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusMessage_dealloc( SvcBusMessage _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusMessage_init(SvcBusMessage _this, bson_oid_p messageId);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusMessage_init_copy(SvcBusMessage _this, SvcBusMessage src);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusMessage_init_Zero( SvcBusMessage _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusMessage_destroy(SvcBusMessage _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bson SvcBusMessage_getBson(SvcBusMessage _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bson_oid_p SvcBusMessage_get__id( SvcBusMessage _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bson_oid_p SvcBusMessage_get_messageId( SvcBusMessage _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusMessage_get_responsePipeName( SvcBusMessage _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte SvcBusMessage_get_broadcast( SvcBusMessage _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusMessage_get_command(SvcBusMessage _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusMessage_set_broadcast(SvcBusMessage _this, byte broadcast);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusMessage_set_ttl(SvcBusMessage _this, uint ttl);

    #endregion

    #region SvcBusPipeService_DLLImports
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusPipeService SvcBusPipeService_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusPipeService_dealloc( SvcBusPipeService _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusPipeService_init(SvcBusPipeService _this, [MarshalAs(UnmanagedType.LPStr)]string connectionString);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusPipeService_destroy( SvcBusPipeService _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusPipeService_releaseDeadResponsePipes( SvcBusPipeService _this, int heartbeatMs );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusPipeService_getlastErrorCode(SvcBusPipeService _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusPipeService_getlastErrorMsg(SvcBusPipeService _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusPipeService_startAsyncWorkPool( SvcBusPipeService _this, uint minThreads, uint maxThreads );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusPipeService_stopAsyncWorkPool( SvcBusPipeService _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusPipeService_configureConnectionPoolCleanupTimer( SvcBusPipeService _this, uint timerIntervalMillis, uint maxIdleMillis );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusStatsCollector SvcBusPipeService_getStatsCollector(SvcBusPipeService _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusPipeService_dropServicePipe(SvcBusPipeService _this, SvcBusService svc);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusPipeService_dropPipe(SvcBusPipeService _this, [MarshalAs(UnmanagedType.LPStr)]string pipeName);

    #endregion

    #region SvcBusService_DLLImports
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusService SvcBusService_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusService_dealloc( SvcBusService _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusService_init(SvcBusService _this, [MarshalAs(UnmanagedType.LPStr)]string name, int mode, uint volatileSize, uint responsePipeSize);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusService_destroy(SvcBusService _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusService_getName(SvcBusService _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusService_getMode(SvcBusService _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern  uint SvcBusService_getvolatileSize(SvcBusService _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SvcBusService_getresponsePipeSize(SvcBusService _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusService_getlastErrorCode(SvcBusService _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusService_getlastErrorMsg(SvcBusService _this);
    #endregion

    #region SvcBusServicePersistence_DLLImports

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusServicePersistence SvcBusServicePersistence_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusServicePersistence_dealloc(SvcBusServicePersistence _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusServicePersistence_init(SvcBusServicePersistence _this, SvcBusPipeService ps);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusServicePersistence_destroy(SvcBusServicePersistence _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusServicePersistence_save(SvcBusServicePersistence _this, SvcBusService service);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusServicePersistence_remove(SvcBusServicePersistence _this, string serviceName);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusServicePersistence_load(SvcBusServicePersistence _this, SvcBusService service, string serviceName);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusServicePersistence_exists(SvcBusServicePersistence _this, string serviceName);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusServicePersistence_getlastErrorCode(SvcBusServicePersistence _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusServicePersistence_getlastErrorMsg(SvcBusServicePersistence _this);
    #endregion

    #region SvcBusConsumer_DLLImports
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusConsumer SvcBusConsumer_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumer_dealloc( SvcBusConsumer _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusConsumer_init(SvcBusConsumer _this, SvcBusPipeService ps, [MarshalAs(UnmanagedType.LPStr)]string name, SvcBusService service, int options);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusConsumer_initBindToResponsePipe(SvcBusConsumer _this, SvcBusPipeService ps, [MarshalAs(UnmanagedType.LPStr)]string name, int options,
      [MarshalAs(UnmanagedType.LPStr)]string responsePipeName);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusConsumer_getResponsePipeName(SvcBusConsumer _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumer_destroy( SvcBusConsumer _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_send( SvcBusConsumer _this, SvcBusMessage message );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_wait( SvcBusConsumer _this, ref bson result, bson_oid_p msgId );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_wait_multiple(SvcBusConsumer _this, ref bson result, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]bson_oid_p[] msgIds);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_sendAndWait( SvcBusConsumer _this, ref bson result, SvcBusMessage message );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_updateHeartbeat(SvcBusConsumer _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_getlastErrorCode(SvcBusConsumer _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusConsumer_getlastErrorMsg(SvcBusConsumer _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_setResponsePipeTimeout(SvcBusConsumer _this, uint timeout);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumer_setMaxRetriesOnTimeout(SvcBusConsumer _this, uint maxRetries);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_sendBroadcast( SvcBusConsumer _this, SvcBusMessage message );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_markBroadcastRequestAsTaken( SvcBusConsumer _this, bson_oid_p id );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_waitBroadcast( SvcBusConsumer _this, ref bson result, bson_oid_p msgId );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_truncateResponseCollection(SvcBusConsumer _this);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SvcBusOnAsyncWait(IntPtr err, bson_oid_p response, IntPtr userdata);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_wait_for_response_async( SvcBusConsumer _this, SvcBusMessage request, SvcBusOnAsyncWait callback, IntPtr callbackUserdata );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumer_startUpdateHeartbeatTimer( SvcBusConsumer _this, uint millis );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumer_stopUpdateHeartbeatTimer(SvcBusConsumer _this);
    #endregion

    #region SvcBusProducer_DLLImports
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusProducer SvcBusProducer_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusProducer_dealloc( SvcBusProducer _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusProducer_init(SvcBusProducer _this, SvcBusPipeService ps, [MarshalAs(UnmanagedType.LPStr)]string name, SvcBusService service);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusProducer_destroy( SvcBusProducer _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusProducer_take( SvcBusProducer _this, SvcBusMessage message );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusProducer_take_by__id(SvcBusProducer _this, bson_oid_p _id);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusProducer_getResponder(SvcBusProducer _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusProducer_wait( SvcBusProducer _this, SvcBusMessage result );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusProducer_getlastErrorCode( SvcBusProducer _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusProducer_getlastErrorMsg( SvcBusProducer _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusProducer_setRequestPipeTimeout( SvcBusProducer _this, uint timeout);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusProducer_peek( SvcBusProducer _this, bson result, bson_oid_p id );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusProducer_waitAndTake(SvcBusProducer _this, SvcBusMessage result);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusProducer_poke(SvcBusProducer _this, bson_oid_p id, bson data);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusProducer_remove(SvcBusProducer _this, bson_oid_p id);
    #endregion

    #region SvcBusResponder_DLLImports
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusResponder SvcBusResponder_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusResponder_dealloc(SvcBusResponder _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusResponder_init(SvcBusResponder _this, SvcBusPipeService ps);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusResponder_destroy(SvcBusResponder _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusResponder_send(SvcBusResponder _this, [MarshalAs(UnmanagedType.LPStr)]string responsePipeName, SvcBusMessage message);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusResponder_getlastErrorCode(SvcBusResponder _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusResponder_getlastErrorMsg(SvcBusResponder _this);
    #endregion

    #region SvcBusConsumerPool_DLLImports
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusConsumerPool SvcBusConsumerPool_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumerPool_dealloc(SvcBusConsumerPool _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusConsumerPool_init(SvcBusConsumerPool _this, SvcBusConsumer ps, [MarshalAs(UnmanagedType.LPStr)]string name, 
                                                     SvcBusService service, int  options);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumerPool_destroy(SvcBusConsumerPool _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusGenericErr SvcBusConsumerPool_getErr(SvcBusConsumerPool _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusConsumer SvcBusConsumerPool_acquire(SvcBusConsumerPool _this, SvcBusGenericErr err);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumerPool_release(SvcBusConsumerPool _this, SvcBusConsumer consumer);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumerPool_setCopy(SvcBusConsumerPool _this, byte copy);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumerPool_autoStartHeartbeatTimer(SvcBusConsumerPool _this, uint milis);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumerPool_startAsyncWorkPool(SvcBusConsumerPool _this,  uint minThreads, uint maxThreads);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumerPool_stopAsyncWorkPool(SvcBusConsumerPool _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumerPool_configureConsumerPoolCleanupTimer(SvcBusConsumerPool _this,
                                                                                  uint timerIntervalMillis, uint maxIdleMillis);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusConsumerPoolDictionary SvcBusConsumerPoolDictionary_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumerPoolDictionary_dealloc(SvcBusConsumerPoolDictionary _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusConsumerPoolDictionary_init(SvcBusConsumerPoolDictionary _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusConsumerPoolDictionary_destroy(SvcBusConsumerPoolDictionary _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusGenericErr SvcBusConsumerPoolDictionary_getErr(SvcBusConsumerPoolDictionary _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern SvcBusConsumerPool SvcBusConsumerPoolDictionary_getConsumerPool(SvcBusConsumerPoolDictionary _this, 
                                                                                         [MarshalAs(UnmanagedType.LPStr)]string connectionString,
                                                                                         [MarshalAs(UnmanagedType.LPStr)]string serviceName,
                                                                                         [MarshalAs(UnmanagedType.LPStr)]string consumerName,
                                                                                         int options, SvcBusGenericErr err);
    #endregion

    #region SvcBusThreadPool_DLLImports

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SvcBusThreadPoolCallbackDelegate(IntPtr context);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusThreadPool SvcBusThreadPool_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusThreadPool_dealloc( SvcBusThreadPool _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusThreadPool_init( SvcBusThreadPool _this, uint minThreads, uint maxThreads );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusThreadPool_destroy( SvcBusThreadPool _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusThreadWork SvcBusThreadWork_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusThreadWork_dealloc( SvcBusThreadWork _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusThreadWork_init(SvcBusThreadWork _this, SvcBusThreadPool pool, SvcBusThreadPoolCallbackDelegate callback, IntPtr context);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusThreadWork_destroy( SvcBusThreadWork _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusThreadWork_submit( SvcBusThreadWork _this, SvcBusThreadPool pool );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusThreadWork_wait( SvcBusThreadWork _this );

    #endregion

    #region SvcBusTimerManagement
    /********************/
    /* Timer Management */
    /********************/

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusThreadPoolTimer SvcBusThreadPoolTimer_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusThreadPoolTimer_dealloc( SvcBusThreadPoolTimer _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusThreadPoolTimer_init( SvcBusThreadPoolTimer _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusThreadPoolTimer_destroy( SvcBusThreadPoolTimer _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusThreadPoolTimer_start(SvcBusThreadPoolTimer _this, SvcBusThreadPool pool, SvcBusThreadPoolCallbackDelegate callback, IntPtr context, uint millis);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusThreadPoolTimer_stop( SvcBusThreadPoolTimer _this );

    #endregion

    #region SvcBusMutex
    
    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern SvcBusMutex SvcBusMutex_alloc();

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusMutex_dealloc( SvcBusMutex _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int SvcBusMutex_init(SvcBusMutex _this, SvcBusPipeService ps, 
                                              [MarshalAs(UnmanagedType.LPStr)]string name,
                                              [MarshalAs(UnmanagedType.LPStr)]string svcname);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusMutex_destroy( SvcBusMutex _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern svc_bus_mutex_operation_result_t SvcBusMutex_acquire( SvcBusMutex _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern svc_bus_mutex_operation_result_t SvcBusMutex_release( SvcBusMutex _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern svc_bus_mutex_operation_result_t SvcBusMutex_forceRelease(SvcBusMutex _this,
                                                                                   [MarshalAs(UnmanagedType.LPStr)]string computerName,
                                                                                   [MarshalAs(UnmanagedType.LPStr)]string procName,
                                                                                   Int32 procId);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusMutex_lockedMutexQueryInit( SvcBusMutex _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusMutex_lockedMutexQueryDone( SvcBusMutex _this );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SvcBusMutex_lockedMutexQueryNext( SvcBusMutex _this, out bson curr );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusMutex_getlastErrorCode(SvcBusMutex _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SvcBusMutex_getlastErrorMsg(SvcBusMutex _this);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SvcBusMutex_remove(SvcBusMutex _this);

    #endregion

    #region SvcBusStatsCollector

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusStatsCollector_setFlushInterval( SvcBusStatsCollector _this, UInt32 millis );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern void SvcBusStatsCollector_send_report(SvcBusStatsCollector _this,
                                                                [MarshalAs(UnmanagedType.LPStr)]string statTags,
                                                                [MarshalAs(UnmanagedType.LPStr)]string statValues);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern void SvcBusStatsCollector_report_init( SvcBusStatsCollector _this,
                                                                bson_oid_p report_id,
                                                                [MarshalAs(UnmanagedType.LPStr)]string stat_tags,
                                                                [MarshalAs(UnmanagedType.LPStr)]string stat_values,
                                                                UInt32 timeout );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusStatsCollector_cancel_report( SvcBusStatsCollector _this,
                                                                  bson_oid_p report_id );

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern void SvcBusStatsCollector_report_complete_by_time_elapsed( SvcBusStatsCollector _this,
                                                                                    bson_oid_p report_id,
                                                                                    [MarshalAs(UnmanagedType.LPStr)]string stat_values);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern void SvcBusStatsCollector_report_complete_timeout( SvcBusStatsCollector _this,
                                                                            bson_oid_p report_id,
                                                                            [MarshalAs(UnmanagedType.LPStr)]string stat_values);

    [DllImport(SvcBusDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SvcBusStatsCollector_setEnabled(SvcBusStatsCollector _this, byte enabled);

    #endregion
  }
}
