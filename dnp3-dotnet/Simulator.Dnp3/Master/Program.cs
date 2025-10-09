using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using dnp3;

namespace Simulator.Dnp3.Master
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ANCHOR: logging_init
            // Initialize logging with the default configuration
            // This may only be called once during program initialization
            Logging.Configure(
                new LoggingConfig(),
                new ConsoleLogger()
            );
            // ANCHOR_END: logging_init
            
            Console.WriteLine("env");
            var transport = Environment.GetEnvironmentVariable("TRANSPORT");
            var outstationIp = Environment.GetEnvironmentVariable("OUTSTATION_IP");
            var outstationPort = Environment.GetEnvironmentVariable("OUTSTATION_PORT");

            // ANCHOR: runtime_init
            var runtime = new Runtime(new RuntimeConfig { NumCoreThreads = 4 });
            // ANCHOR_END: runtime_init

            if (string.IsNullOrWhiteSpace(transport) && args.Length != 1)
            {
                System.Console.WriteLine("You must specify the transport type");
                System.Console.WriteLine("Usage: master-example <transport> (tcp, serial, tls-ca, tls-self-signed)");
                return;
            }

            try
            {
                transport ??= args[0];
                var type = transport;
                switch (type)
                {
                    case "tcp":
                        RunTcp(runtime, outstationIp, outstationPort);
                        break;
                    case "udp":
                        RunUdp(runtime);
                        break;
                    case "serial":
                        RunSerial(runtime);
                        break;
                    case "tls-ca":
                        RunTls(runtime, GetCaTlsConfig());
                        break;
                    case "tls-self-signed":
                        RunTls(runtime, GetSelfSignedTlsConfig());
                        break;
                    default:
                        System.Console.WriteLine("Unknown transport: %s", type);
                        break;
                }
            }
            finally
            {
                // ANCHOR: runtime_shutdown
                runtime.Shutdown();
                // ANCHOR_END: runtime_shutdown
            }
        }

        // ANCHOR: master_channel_config
        private static MasterChannelConfig GetMasterChannelConfig()
        {
            return new MasterChannelConfig(1)
                .WithDecodeLevel(DecodeLevel.Nothing().WithApplication(AppDecodeLevel.ObjectValues));
        }
        // ANCHOR_END: master_channel_config

        // ANCHOR: association_config
        private static AssociationConfig GetAssociationConfig()
        {
            return new AssociationConfig(
                    // disable unsolicited first (Class 1/2/3)
                    EventClasses.All(),
                    // after the integrity poll, enable unsolicited (Class 1/2/3)
                    EventClasses.All(),
                    // perform startup integrity poll with Class 1/2/3/0
                    Classes.All(),
                    // don't automatically scan Class 1/2/3 when the corresponding IIN bit is asserted
                    EventClasses.None()
                )
                .WithAutoTimeSync(AutoTimeSync.Lan)
                .WithKeepAliveTimeout(TimeSpan.FromSeconds(60));
        }
        // ANCHOR_END: association_config

        private static void RunTcp(Runtime runtime, string? ip = "127.0.0.1", string? port = "20000")
        {
            // ANCHOR: create_tcp_channel
            var channel = MasterChannel.CreateTcpChannel(
                runtime,
                LinkErrorMode.Close,
                GetMasterChannelConfig(),
                new EndpointList($"{ip}:{port}"),
                new ConnectStrategy(),
                new TestClientStateListener()
            );
            // ANCHOR_END: create_tcp_channel

            try
            {
                RunChannel(channel);
            }
            finally
            {
                channel.Shutdown();
            }
        }

        private static void RunTls(Runtime runtime, TlsClientConfig tlsConfig)
        {
            // ANCHOR: create_tls_channel
            var channel = MasterChannel.CreateTlsChannel(
                runtime,
                LinkErrorMode.Close,
                GetMasterChannelConfig(),
                new EndpointList("127.0.0.1:20001"),
                new ConnectStrategy(),
                new TestClientStateListener(),
                tlsConfig
            );
            // ANCHOR_END: create_tls_channel

            try
            {
                RunChannel(channel);
            }
            finally
            {
                channel.Shutdown();
            }
        }

        private static void RunUdp(Runtime runtime)
        {
            // ANCHOR: create_udp_channel
            var channel = MasterChannel.CreateUdpChannel(
                runtime,
                GetMasterChannelConfig(),
                "127.0.0.1:20001",
                LinkReadMode.Datagram,
                TimeSpan.FromSeconds(5)
            );
            // ANCHOR_END: create_serial_channel

            try
            {
                // ANCHOR: association_create_udp
                var association = channel.AddUdpAssociation(
                    1024,
                    "127.0.0.1:20000",
                    GetAssociationConfig(),
                    new TestReadHandler(),
                    new TestAssociationHandler(),
                    new TestAssociationInformation()
                );
                // ANCHOR_END: association_create_udp

                RunAssociation(channel, association);
            }
            finally
            {
                channel.Shutdown();
            }
        }

        private static void RunSerial(Runtime runtime)
        {
            // ANCHOR: create_serial_channel
            var channel = MasterChannel.CreateSerialChannel(
                runtime,
                GetMasterChannelConfig(),
                "COM1",
                new SerialSettings(),
                TimeSpan.FromSeconds(5),
                new TestPortStateListener()
            );
            // ANCHOR_END: create_serial_channel

            try
            {
                RunChannel(channel);
            }
            finally
            {
                channel.Shutdown();
            }
        }

        private static TlsClientConfig GetCaTlsConfig()
        {
            // ANCHOR: tls_ca_chain_config
            // defaults to CA mode
            var config = new TlsClientConfig(
                "test.com",
                "./certs/ca_chain/ca_cert.pem",
                "./certs/ca_chain/entity1_cert.pem",
                "./certs/ca_chain/entity1_key.pem",
                "" // no password
            );
            // ANCHOR_END: tls_ca_chain_config
            return config;
        }

        private static TlsClientConfig GetSelfSignedTlsConfig()
        {
            // ANCHOR: tls_self_signed_config
            var config = new TlsClientConfig(
                "test.com",
                "./certs/self_signed/entity2_cert.pem",
                "./certs/self_signed/entity1_cert.pem",
                "./certs/self_signed/entity1_key.pem",
                "" // no password
            ).WithCertificateMode(CertificateMode.SelfSigned);
            // ANCHOR_END: tls_self_signed_config
            return config;
        }


        private static void RunAssociation(MasterChannel channel, AssociationId association)
        {
            // ANCHOR: add_poll
            var poll = channel.AddPoll(association, Request.ClassRequest(false, true, true, true),
                TimeSpan.FromSeconds(5));
            // ANCHOR_END: add_poll

            // start communications
            channel.Enable();

            while (true)
            {
                var input = Console.ReadLine();

                if (input == "x")
                {
                    return;
                }

                try
                {
                    RunOneCommand(channel, association, poll, input).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex}");
                }
            }
        }

        private static void RunChannel(MasterChannel channel)
        {
            // ANCHOR: association_create
            var association = channel.AddAssociation(
                1024,
                GetAssociationConfig(),
                new TestReadHandler(),
                new TestAssociationHandler(),
                new TestAssociationInformation()
            );
            // ANCHOR_END: association_create

            RunAssociation(channel, association);
        }

        private static async Task RunOneCommand(MasterChannel channel, AssociationId association, PollId poll,
            String input)
        {
            switch (input)
            {
                case "enable":
                {
                    channel.Enable();
                    break;
                }
                case "disable":
                {
                    channel.Disable();
                    break;
                }
                case "dln":
                {
                    channel.SetDecodeLevel(DecodeLevel.Nothing());
                    break;
                }
                case "dlv":
                {
                    channel.SetDecodeLevel(DecodeLevel.Nothing().WithApplication(AppDecodeLevel.ObjectValues));
                    break;
                }
                case "rao":
                {
                    var request = new Request();
                    request.AddAllObjectsHeader(Variation.Group40Var0);
                    await channel.Read(association, request);
                    break;
                }
                case "rmo":
                {
                    var request = new Request();
                    request.AddAllObjectsHeader(Variation.Group10Var0);
                    request.AddAllObjectsHeader(Variation.Group40Var0);
                    await channel.Read(association, request);
                    break;
                }
                case "cmd":
                {
                    // ANCHOR: assoc_control
                    var commands = new CommandSet();
                    commands.AddG12V1U8(3, Group12Var1.FromCode(ControlCode.FromOpType(OpType.LatchOn)));
                    await channel.Operate(association, CommandMode.SelectBeforeOperate, commands);
                    // ANCHOR_END: assoc_control
                    break;
                }
                case "evt":
                {
                    channel.DemandPoll(poll);
                    break;
                }
                case "lts":
                {
                    await channel.SynchronizeTime(association, TimeSyncMode.Lan);
                    break;
                }
                case "nts":
                {
                    await channel.SynchronizeTime(association, TimeSyncMode.NonLan);
                    break;
                }
                case "wad":
                {
                    var request = new WriteDeadBandRequest();
                    request.AddG34v1U8(3, 5);
                    request.AddG34v3U16(4, 2.5f);
                    await channel.WriteDeadBands(association, request);
                    break;
                }
                case "fat":
                {
                    var request = new Request();
                    request.AddTimeAndInterval(0, 86400000);
                    request.AddAllObjectsHeader(Variation.Group20Var0);
                    await channel.SendAndExpectEmptyResponse(association, FunctionCode.FreezeAtTime, request);
                    break;
                }
                case "rda":
                {
                    // ANCHOR: read_attributes
                    var request = new Request();
                    request.AddSpecificAttribute(AttributeVariations.AllAttributesRequest, 0);
                    await channel.Read(association, request);
                    // ANCHOR_END: read_attributes
                    break;
                }
                case "wda":
                {
                    // ANCHOR: write_attribute
                    var request = new Request();
                    request.AddStringAttribute(AttributeVariations.UserAssignedLocation, 0, "Mt. Olympus");
                    await channel.SendAndExpectEmptyResponse(association, FunctionCode.Write, request);
                    // ANCHOR_END: write_attribute
                    break;
                }
                case "ral":
                {
                    var request = new Request();
                    request.AddSpecificAttribute(AttributeVariations.ListOfVariations, 0);
                    await channel.Read(association, request);
                    break;
                }
                case "rd":
                {
                    // ANCHOR: read_directory
                    var items = await channel.ReadDirectory(association, ".", DirReadConfig.Defaults());
                    foreach (var info in items)
                    {
                        PrintFileInfo(info);
                    }

                    // ANCHOR_END: read_directory
                    break;
                }
                case "gfi":
                {
                    // ANCHOR: get_file_info
                    var info = await channel.GetFileInfo(association, ".");
                    PrintFileInfo(info);
                    // ANCHOR_END: get_file_info
                    break;
                }
                case "rf":
                {
                    // ANCHOR: read_file
                    channel.ReadFile(association, ".", FileReadConfig.Defaults(), new FileReader());
                    // ANCHOR_END: read_file
                    break;
                }
                case "wf":
                {
                    // ANCHOR: write_file
                    var file = await channel.OpenFile(association, "hello_world.txt", 0, Permissions.None(), 0xFFFFFFFF,
                        dnp3.FileMode.Write, 1024);
                    await channel.WriteFileBlock(association, file.FileHandle, 0, false,
                        System.Text.Encoding.UTF8.GetBytes("hello world\n"));
                    await channel.WriteFileBlock(association, file.FileHandle, 1, true,
                        System.Text.Encoding.UTF8.GetBytes("hello world\n"));
                    await channel.CloseFile(association, file.FileHandle);
                    // ANCHOR_END: write_file
                    break;
                }
                case "crt":
                {
                    var delay = await channel.ColdRestart(association);
                    Console.WriteLine($"Restart delay: {delay}");
                    break;
                }
                case "wrt":
                {
                    var delay = await channel.WarmRestart(association);
                    Console.WriteLine($"Restart delay: {delay}");
                    break;
                }
                case "lsr":
                {
                    var result = await channel.CheckLinkStatus(association);
                    Console.WriteLine($"Result: {result}");
                    break;
                }
                default:
                    Console.WriteLine("Unknown command: {0}", input);
                    break;
            }
        }

        private static void PrintFileInfo(dnp3.FileInfo info)
        {
            Console.WriteLine($"Filename: {info.FileName}");
            Console.WriteLine($"  type: {info.FileType}");
            Console.WriteLine($"  size: {info.Size}");
            Console.WriteLine($"  created: {info.TimeCreated}");
        }
    }
}