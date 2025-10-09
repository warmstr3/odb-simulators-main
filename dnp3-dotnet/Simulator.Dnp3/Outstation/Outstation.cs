using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using dnp3;
using Simulator.Dnp3.Common;

namespace Simulator.Dnp3.Outstation
{
    
    public abstract class OutStationBase(OutstationDeviceConfig outstationConfig)
    {
        protected OutstationDeviceConfig OutstationConfig { get; set; } = outstationConfig;
        
        private EventBufferConfig GetEventBufferConfig()
        {
            return new EventBufferConfig(
                10, // binary
                10, // double-bit binary
                10, // binary output status
                5, // counter
                5, // frozen counter
                5, // analog
                5, // analog output status
                3 // octet string
            );
        }
        
        private OutstationConfig GetOutstationConfig()
        {
            // ANCHOR: outstation_config
            // create an outstation configuration with default values
            var config = new OutstationConfig(
                // outstation address
                1024,
                // master address
                1,
                // event buffer sizes
                GetEventBufferConfig()
            ).WithDecodeLevel(DecodeLevel.Nothing().WithApplication(AppDecodeLevel.ObjectValues));
            // ANCHOR_END: outstation_config
            return config;
        }
        
        private void RunServer(OutstationServer server)
        {
            // ANCHOR: tcp_server_add_outstation
            var outstation = server.AddOutstation(
                GetOutstationConfig(),
                new TestOutstationApplication(),
                new TestOutstationInformation(),
                new TestControlHandler(),
                new TestConnectionStateListener(),
                AddressFilter.Any()
            );
            // ANCHOR_END: tcp_server_add_outstation

            // ANCHOR: tcp_server_bind
            server.Bind();
            // ANCHOR_END: tcp_server_bind

            RunOutstation(outstation);
        }
        
        private void RunOutstation(dnp3.Outstation outstation)
        {
            // Setup initial points
            // ANCHOR: database_init
            outstation.Transaction(db =>
            {
                foreach (var config in OutstationConfig.Analog)
                {
                    db.AddAnalogInput(ushort.Parse(config.Key), EventClass.Class1, new AnalogInputConfig());
                }
                foreach (var config in OutstationConfig.Binary)
                {
                    db.AddBinaryInput(ushort.Parse(config.Key), EventClass.Class1, new BinaryInputConfig());
                }
                foreach (var config in OutstationConfig.BinaryOutputStatus)
                {
                    db.AddBinaryOutputStatus(ushort.Parse(config.Key), EventClass.Class1, new BinaryOutputStatusConfig());
                }
                // // add 10 points of each type
                // for (ushort i = 0; i < 10; i++)
                // {
                //     // you can explicitly specify the configuration for each point ...
                //     db.AddBinaryInput(i, EventClass.Class1,
                //         new BinaryInputConfig(StaticBinaryInputVariation.Group1Var1,
                //             EventBinaryInputVariation.Group2Var2)
                //     );
                //     // ... or just use the defaults
                //     db.AddDoubleBitBinaryInput(i, EventClass.Class1, new DoubleBitBinaryInputConfig());
                //     db.AddBinaryOutputStatus(i, EventClass.Class1, new BinaryOutputStatusConfig());
                //     db.AddCounter(i, EventClass.Class1, new CounterConfig());
                //     db.AddFrozenCounter(i, EventClass.Class1, new FrozenCounterConfig());
                //     db.AddAnalogInput(i, EventClass.Class1, new AnalogInputConfig());
                //     db.AddAnalogOutputStatus(i, EventClass.Class1, new AnalogOutputStatusConfig());
                //     db.AddOctetString(i, EventClass.Class1);
                // }

                // define device attributes made available to the master
                db.DefineStringAttr(0, false, AttributeVariations.DeviceManufacturersName, "Step Function I/O");
                db.DefineStringAttr(0, true, AttributeVariations.UserAssignedLocation, "Bend, OR");
            });
            // ANCHOR_END: database_init

            var binaryValues = OutstationConfig.Binary.ToBinaryValues();
            var doubleBitBinaryValue = DoubleBit.DeterminedOff;
            var binaryOutputStatusValues = OutstationConfig.BinaryOutputStatus.ToBinaryValues();;
            var counterValue = (uint)0;
            var frozenCounterValue = (uint)0;
            var analogValues = OutstationConfig.Analog.ToUShortIndex();
            var analogOutputStatusValue = 0.0;

            var onlineFlags = new Flags(Flag.Online);
            var detectEvent = UpdateOptions.DetectEvent();

            outstation.Transaction(db =>
            {
                foreach (var point in binaryOutputStatusValues)
                {
                    db.UpdateBinaryOutputStatus(new BinaryOutputStatus(point.Key, binaryOutputStatusValues[point.Key], onlineFlags, Utility.Now()),
                        detectEvent);
                }
                foreach (var point in binaryValues)
                {
                    db.UpdateBinaryInput(new BinaryInput(point.Key, binaryValues[point.Key], onlineFlags, Utility.Now()),
                        detectEvent);
                }
                foreach (var point in analogValues)
                {
                    db.UpdateAnalogInput(new AnalogInput(point.Key, analogValues[point.Key], onlineFlags, Utility.Now()),
                        detectEvent);
                }
            });

            while (true)
            {
                var input = Console.ReadLine();
                switch (input)
                {
                    case "x":
                        return;
                    case "enable":
                    {
                        outstation.Enable();
                        break;
                    }
                    case "disable":
                    {
                        outstation.Disable();
                        break;
                    }
                    case "bi":
                    {
                        outstation.Transaction(db =>
                        {
                            db.UpdateBinaryInput(new BinaryInput(7, binaryValues[7], onlineFlags, Utility.Now()),
                                detectEvent);
                        });
                        break;
                    }
                    case "dbbi":
                    {
                        outstation.Transaction(db =>
                        {
                            doubleBitBinaryValue = doubleBitBinaryValue == DoubleBit.DeterminedOff
                                ? DoubleBit.DeterminedOn
                                : DoubleBit.DeterminedOff;
                            db.UpdateDoubleBitBinaryInput(
                                new DoubleBitBinaryInput(7, doubleBitBinaryValue, onlineFlags, Utility.Now()),
                                detectEvent);
                        });
                        break;
                    }
                    case "bos":
                    {
                        outstation.Transaction(db =>
                        {
                            db.UpdateBinaryOutputStatus(
                                new BinaryOutputStatus(7, binaryOutputStatusValues[7], onlineFlags, Utility.Now()),
                                detectEvent);
                        });
                        break;
                    }
                    case "co":
                    {
                        outstation.Transaction(db =>
                        {
                            db.UpdateCounter(new Counter(7, ++counterValue, onlineFlags, Utility.Now()),
                                detectEvent);
                        });
                        break;
                    }
                    case "fco":
                    {
                        outstation.Transaction(db =>
                        {
                            db.UpdateFrozenCounter(
                                new FrozenCounter(7, ++frozenCounterValue, onlineFlags, Utility.Now()),
                                detectEvent);
                        });
                        break;
                    }
                    case "ai":
                    {
                        outstation.Transaction(db =>
                        {
                            db.UpdateAnalogInput(new AnalogInput(7, analogValues[7], onlineFlags, Utility.Now()),
                                detectEvent);
                        });
                        break;
                    }
                    case "aif":
                    {
                        outstation.Transaction(db =>
                        {
                            db.UpdateFlags(7, UpdateFlagsType.AnalogInput, new Flags(Flag.CommLost), Utility.Now(),
                                detectEvent);
                        });
                        break;
                    }
                    case "aos":
                    {
                        outstation.Transaction(db =>
                        {
                            db.UpdateAnalogOutputStatus(
                                new AnalogOutputStatus(7, ++analogOutputStatusValue, onlineFlags, Utility.Now()),
                                detectEvent);
                        });
                        break;
                    }
                    case "os":
                    {
                        outstation.Transaction(db =>
                        {
                            db.UpdateOctetString(7, System.Text.Encoding.ASCII.GetBytes("Hello"), detectEvent);
                        });
                        break;
                    }
                    default:
                        Console.WriteLine("Unknown command: {0}", input);
                        break;
                }
            }
        }
        
        private void RunTcp(Runtime runtime)
        {
            // ANCHOR: create_tcp_server
            var server = OutstationServer.CreateTcpServer(runtime, LinkErrorMode.Close, $"0.0.0.0:20000");
            // ANCHOR_END: create_tcp_server

            try
            {
                RunServer(server);
            }
            finally
            {
                server.Shutdown();
            }
        }
        
        private void RunSerial(Runtime runtime)
        {
            // ANCHOR: create_serial_server
            var outstation = dnp3.Outstation.CreateSerialSession2(
                runtime,
                "COM1",
                new SerialSettings(),
                TimeSpan.FromSeconds(5), // try to open the port every 5 seconds
                GetOutstationConfig(),
                new TestOutstationApplication(),
                new TestOutstationInformation(),
                new TestControlHandler(),
                new TestPortStateListener()
            );
            // ANCHOR_END: create_serial_server


            RunOutstation(outstation);
        }

        private void RunUdp(Runtime runtime)
        {
            // ANCHOR: create_udp
            var config = new OutstationUdpConfig(
                "127.0.0.1:20000",
                "127.0.0.1:20001"
            );

            var outstation = dnp3.Outstation.CreateUdp(
                runtime,
                config,
                GetOutstationConfig(),
                new TestOutstationApplication(),
                new TestOutstationInformation(),
                new TestControlHandler()
            );
            // ANCHOR_END: create_udp

            RunOutstation(outstation);
        }

        private void RunTls(Runtime runtime, TlsServerConfig config)
        {
            // ANCHOR: create_tls_server
            var server = OutstationServer.CreateTlsServer(runtime, LinkErrorMode.Close, "127.0.0.1:20001", config);
            // ANCHOR_END: create_tls_server

            try
            {
                RunServer(server);
            }
            finally
            {
                server.Shutdown();
            }
        }
        
        private TlsServerConfig GetCaTlsConfig()
        {
            // ANCHOR: tls_ca_chain_config
            var config = new TlsServerConfig(
                "test.com",
                "./certs/ca_chain/ca_cert.pem",
                "./certs/ca_chain/entity2_cert.pem",
                "./certs/ca_chain/entity2_key.pem",
                "" // no password
            );
            // ANCHOR_END: tls_ca_chain_config
            return config;
        }

        private TlsServerConfig GetSelfSignedTlsConfig()
        {
            // ANCHOR: tls_self_signed_config
            var config = new TlsServerConfig(
                "test.com",
                "./certs/self_signed/entity1.pem",
                "./certs/self_signed/entity2_cert.pem",
                "./certs/self_signed/entity2_key.pem",
                "" // no password
            ).WithCertificateMode(CertificateMode.SelfSigned);
            // ANCHOR_END: tls_self_signed_config
            return config;
        }

        public void Run(string transport)
        {
            var runtime = new Runtime(new RuntimeConfig { NumCoreThreads = 4 });
            try
            {
                switch (transport)
                {
                    case "tcp":
                        RunTcp(runtime);
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
                        Console.WriteLine("Unknown transport: {0}", transport);
                        break;
                }
            }
            finally
            {
                runtime.Shutdown();
            }
        }
    }

    public class Sel651R2(OutstationDeviceConfig outstationConfig) : OutStationBase(outstationConfig);
    
    public class Sel734P(OutstationDeviceConfig outstationConfig) : OutStationBase(outstationConfig);
}