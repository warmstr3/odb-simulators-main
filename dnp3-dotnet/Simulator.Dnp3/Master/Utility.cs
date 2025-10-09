using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using dnp3;

namespace Simulator.Dnp3.Master
{
    // ANCHOR: logging_interface
    // callback interface used to receive log messages
    class ConsoleLogger : ILogger
    {
        public void OnMessage(LogLevel level, string message)
        {
            Console.Write($"{message}");
        }
    }
    // ANCHOR_END: logging_interface

    class TestClientStateListener : IClientStateListener
    {
        public void OnChange(ClientState state)
        {
            Console.WriteLine(state);
        }
    }

    class TestPortStateListener : IPortStateListener
    {
        public void OnChange(PortState state)
        {
            Console.WriteLine(state);
        }
    }

    // ANCHOR: read_handler
    class TestReadHandler : IReadHandler
    {
        public void BeginFragment(ReadType readType, ResponseHeader header)
        {
            Console.WriteLine($"Beginning fragment (broadcast: {header.Iin.Iin1.Broadcast})");
        }

        public void EndFragment(ReadType readType, ResponseHeader header)
        {
            Console.WriteLine("End fragment");
        }

        public void HandleBinaryInput(HeaderInfo info, ICollection<BinaryInput> values)
        {
            Console.WriteLine("Binary Inputs:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine(
                    $"BI {val.Index}: Value={val.Value} Flags={val.Flags.Value} Time={val.Time.Value} ({val.Time.Quality})");
            }
        }

        public void HandleDoubleBitBinaryInput(HeaderInfo info, ICollection<DoubleBitBinaryInput> values)
        {
            Console.WriteLine("Double Bit Binary Inputs:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine(
                    $"DBBI {val.Index}: Value={val.Value} Flags={val.Flags.Value} Time={val.Time.Value} ({val.Time.Quality})");
            }
        }

        public void HandleBinaryOutputStatus(HeaderInfo info, ICollection<BinaryOutputStatus> values)
        {
            Console.WriteLine("Binary Output Statuses:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine(
                    $"BOS {val.Index}: Value={val.Value} Flags={val.Flags.Value} Time={val.Time.Value} ({val.Time.Quality})");
            }
        }

        public void HandleCounter(HeaderInfo info, ICollection<Counter> values)
        {
            Console.WriteLine("Counters:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine(
                    $"Counter {val.Index}: Value={val.Value} Flags={val.Flags.Value} Time={val.Time.Value} ({val.Time.Quality})");
            }
        }

        public void HandleFrozenCounter(HeaderInfo info, ICollection<FrozenCounter> values)
        {
            Console.WriteLine("Frozen Counters:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine(
                    $"Frozen Counter {val.Index}: Value={val.Value} Flags={val.Flags.Value} Time={val.Time.Value} ({val.Time.Quality})");
            }
        }

        public void HandleAnalogInput(HeaderInfo info, ICollection<AnalogInput> values)
        {
            Console.WriteLine("Analog Inputs:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine(
                    $"AI {val.Index}: Value={val.Value} Flags={val.Flags.Value} Time={val.Time.Value} ({val.Time.Quality})");
            }
        }

        public void HandleFrozenAnalogInput(HeaderInfo info, ICollection<FrozenAnalogInput> values)
        {
            Console.WriteLine("Frozen Analog Inputs:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine(
                    $"AI {val.Index}: Value={val.Value} Flags={val.Flags.Value} Time={val.Time.Value} ({val.Time.Quality})");
            }
        }

        public void HandleAnalogOutputStatus(HeaderInfo info, ICollection<AnalogOutputStatus> values)
        {
            Console.WriteLine("Analog Output Statuses:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine(
                    $"AOS {val.Index}: Value={val.Value} Flags={val.Flags.Value} Time={val.Time.Value} ({val.Time.Quality})");
            }
        }

        void IReadHandler.HandleBinaryOutputCommandEvent(HeaderInfo info, ICollection<BinaryOutputCommandEvent> values)
        {
            Console.WriteLine("Binary Output Command Events:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine(
                    $"BOCE {val.Index}: Value={val.CommandedState} Status={val.Status} Time={val.Time.Value} ({val.Time.Quality})");
            }
        }

        void IReadHandler.HandleAnalogOutputCommandEvent(HeaderInfo info, ICollection<AnalogOutputCommandEvent> values)
        {
            Console.WriteLine("Analog Output Command Events:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine(
                    $"AOCE {val.Index}: Value={val.CommandedValue} Type={val.CommandType} Status={val.Status} Time={val.Time.Value} ({val.Time.Quality})");
            }
        }

        void IReadHandler.HandleUnsignedInteger(HeaderInfo info, ICollection<UnsignedInteger> values)
        {
            Console.WriteLine("Unsigned Integers:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.WriteLine($"{val.Index}: Value={val.Value}");
            }
        }

        public void HandleOctetString(HeaderInfo info, ICollection<OctetString> values)
        {
            Console.WriteLine("Octet Strings:");
            Console.WriteLine("Qualifier: " + info.Qualifier);
            Console.WriteLine("Variation: " + info.Variation);

            foreach (var val in values)
            {
                Console.Write($"Octet String {val.Index}: Value=");
                foreach (var b in val.Value)
                {
                    Console.Write($"{b:X2} ");
                }

                Console.WriteLine();
            }
        }

        void IReadHandler.HandleStringAttr(HeaderInfo info, StringAttr attr, byte set, byte var, string value)
        {
            Console.WriteLine($"Visible string attribute: {attr} set: {set} variation: {var} value: {value}");
        }

        void IReadHandler.HandleUintAttr(HeaderInfo info, UintAttr attr, byte set, byte var, uint value)
        {
            Console.WriteLine($"Unsigned integer attribute: {attr} set: {set} variation: {var} value: {value}");
        }

        void IReadHandler.HandleBoolAttr(HeaderInfo info, BoolAttr attr, byte set, byte var, bool value)
        {
            Console.WriteLine($"Boolean attribute: {attr} set: {set} variation: {var} value: {value}");
        }

        void IReadHandler.HandleIntAttr(HeaderInfo info, IntAttr attr, byte set, byte var, int value)
        {
            Console.WriteLine($"Int attribute: {attr} set: {set} variation: {var} value: {value}");
        }

        void IReadHandler.HandleTimeAttr(HeaderInfo info, TimeAttr attr, byte set, byte var, ulong value)
        {
            Console.WriteLine($"Time attribute: {attr} set: {set} variation: {var} value: {value}");
        }

        void IReadHandler.HandleFloatAttr(HeaderInfo info, FloatAttr attr, byte set, byte var, double value)
        {
            Console.WriteLine($"Float attribute: {attr} set: {set} variation: {var} value: {value}");
        }

        void IReadHandler.HandleVariationListAttr(HeaderInfo info, VariationListAttr attr, byte set, byte var,
            ICollection<AttrItem> value)
        {
            Console.WriteLine($"Attribute variation list: {attr} set: {set} variation: {var}");
            foreach (var item in value)
            {
                Console.WriteLine($"variation: {item.Variation} writable: {item.Properties.IsWritable}");
            }
        }

        void IReadHandler.HandleOctetStringAttr(HeaderInfo info, OctetStringAttr attr, byte set, byte var,
            ICollection<byte> value)
        {
            Console.WriteLine($"Octet-string attribute: {attr} set: {set} variation: {var} length: {value.Count}");
        }

        void IReadHandler.HandleBitStringAttr(HeaderInfo info, BitStringAttr attr, byte set, byte var,
            ICollection<byte> value)
        {
            Console.WriteLine($"Bit-string attribute: {attr} set: {set} variation: {var} length: {value.Count}");
        }
    }
    // ANCHOR_END: read_handler

    // ANCHOR: association_handler
    class TestAssociationHandler : IAssociationHandler
    {
        public UtcTimestamp GetCurrentTime()
        {
            return UtcTimestamp.Valid((ulong)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        }
    }
    // ANCHOR_END: association_handler

    // ANCHOR: association_information
    class TestAssociationInformation : IAssociationInformation
    {
        public void TaskStart(TaskType taskType, FunctionCode fc, byte seq)
        {
        }

        public void TaskSuccess(TaskType taskType, FunctionCode fc, byte seq)
        {
        }

        public void TaskFail(TaskType taskType, TaskError error)
        {
        }

        public void UnsolicitedResponse(bool isDuplicate, byte seq)
        {
        }
    }
    // ANCHOR_END: association_information

    // ANCHOR: file_logger
    class FileReader : IFileReader
    {
        void IFileReader.Aborted(FileError error)
        {
            Console.WriteLine($"File transfer aborted: {error}");
        }

        bool IFileReader.BlockReceived(uint blockNum, ICollection<byte> data)
        {
            Console.WriteLine($"Received file block {blockNum} with size {data.Count}");
            return true;
        }

        void IFileReader.Completed()
        {
            Console.WriteLine($"File transfer completed");
        }

        bool IFileReader.Opened(uint size)
        {
            Console.WriteLine($"Outstation open file with size: ${size}");
            return true;
        }
    }
    // ANCHOR_END: file_logger
}