using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using dnp3;

namespace Simulator.Dnp3.Outstation
{
    
    class TestLogger : ILogger
    {
        public void OnMessage(LogLevel level, string message)
        {
            Console.Write($"{message}");
        }
    }

    class TestOutstationApplication : IOutstationApplication
    {
        public ushort GetProcessingDelayMs()
        {
            return 0;
        }

        public WriteTimeResult WriteAbsoluteTime(ulong time)
        {
            return WriteTimeResult.NotSupported;
        }

        public ApplicationIin GetApplicationIin()
        {
            return new ApplicationIin();
        }

        public RestartDelay ColdRestart()
        {
            return RestartDelay.Seconds(60);
        }

        public RestartDelay WarmRestart()
        {
            return RestartDelay.NotSupported();
        }

        FreezeResult IOutstationApplication.FreezeCountersAll(FreezeType freezeType, DatabaseHandle database) { return FreezeResult.NotSupported; }

        FreezeResult IOutstationApplication.FreezeCountersRange(ushort start, ushort stop, FreezeType freezeType, DatabaseHandle database) { return FreezeResult.NotSupported; }

        FreezeResult IOutstationApplication.FreezeCountersAllAtTime(DatabaseHandle databaseHandle, ulong time, uint interval)
        {
            return FreezeResult.NotSupported;
        }

        FreezeResult IOutstationApplication.FreezeCountersRangeAtTime(ushort start, ushort stop, DatabaseHandle databaseHandle, ulong time, uint interval)
        {
            return FreezeResult.NotSupported;
        }

        bool IOutstationApplication.SupportWriteAnalogDeadBands()
        {
            return false;
        }

        void IOutstationApplication.BeginWriteAnalogDeadBands() {}

        void IOutstationApplication.WriteAnalogDeadBand(ushort index, double deadBand) {}

        void IOutstationApplication.EndWriteAnalogDeadBands() {}

        bool IOutstationApplication.WriteStringAttr(byte set, byte variation, StringAttr attrType, string value)
        {
            // Allow writing any string attributes that have been defined as writable
            return true;
        }

        bool IOutstationApplication.WriteFloatAttr(byte set, byte variation, FloatAttr attrType, float value)
        {            
            return false;
        }

        bool IOutstationApplication.WriteDoubleAttr(byte set, byte variation, FloatAttr attrType, double value)
        {
            return false;
        }

        bool IOutstationApplication.WriteUintAttr(byte set, byte variation, UintAttr attrType, uint value)
        {
            return false;
        }

        bool IOutstationApplication.WriteIntAttr(byte set, byte variation, IntAttr attrType, int value)
        {
            return false;
        }

        bool IOutstationApplication.WriteOctetStringAttr(byte set, byte variation, OctetStringAttr attrType, ICollection<byte> value)
        {
            return false;
        }

        bool IOutstationApplication.WriteBitStringAttr(byte set, byte variation, BitStringAttr attrType, ICollection<byte> value)
        {
            return false;
        }

        bool IOutstationApplication.WriteTimeAttr(byte set, byte variation, TimeAttr attrType, ulong value)
        {
            return false;
        }

        void IOutstationApplication.BeginConfirm()
        {

        }

        void IOutstationApplication.EventCleared(ulong id)
        {

        }

        void IOutstationApplication.EndConfirm(BufferState state)
        {

        }
    }

    class TestOutstationInformation : IOutstationInformation
    {
        public void ProcessRequestFromIdle(RequestHeader header) { }

        public void BroadcastReceived(FunctionCode functionCode, BroadcastAction action) { }

        public void EnterSolicitedConfirmWait(byte ecsn) { }

        public void SolicitedConfirmTimeout(byte ecsn) { }

        public void SolicitedConfirmReceived(byte ecsn) { }

        public void SolicitedConfirmWaitNewRequest() { }

        public void WrongSolicitedConfirmSeq(byte ecsn, byte seq) { }

        public void UnexpectedConfirm(bool unsolicited, byte seq) { }

        public void EnterUnsolicitedConfirmWait(byte ecsn) { }

        public void UnsolicitedConfirmTimeout(byte ecsn, bool retry) { }

        public void UnsolicitedConfirmed(byte ecsn) { }

        public void ClearRestartIin() { }
    }

    // ANCHOR: control_handler
    class TestControlHandler : IControlHandler
    {
        
        public void BeginFragment() { }

        public void EndFragment(DatabaseHandle database) { }

        public CommandStatus SelectG12v1(Group12Var1 control, ushort index, DatabaseHandle database)
        {
            if (index < 10 && (control.Code.OpType == OpType.LatchOn || control.Code.OpType == OpType.LatchOff))
            {
                return CommandStatus.Success;
            }
            else
            {
                return CommandStatus.NotSupported;
            }
        }

        public CommandStatus OperateG12v1(Group12Var1 control, ushort index, OperateType opType, DatabaseHandle database)
        {
            if (index < 10 && (control.Code.OpType == OpType.LatchOn || control.Code.OpType == OpType.LatchOff))
            {
                var status = (control.Code.OpType == OpType.LatchOn);
                database.Transaction(db =>
                    db.UpdateBinaryOutputStatus(new BinaryOutputStatus(index, status, new Flags(Flag.Online), Utility.Now()), UpdateOptions.DetectEvent())
                );
                return CommandStatus.Success;
            }
            else
            {
                return CommandStatus.NotSupported;
            }
        }

        public CommandStatus SelectG41v1(int value, ushort index, DatabaseHandle database)
        {
            return SelectAnalogOutput(index);
        }

        public CommandStatus OperateG41v1(int value, ushort index, OperateType opType, DatabaseHandle database)
        {
            return OperateAnalogOutput(value, index, database);
        }

        public CommandStatus SelectG41v2(short value, ushort index, DatabaseHandle database)
        {
            return SelectAnalogOutput(index);
        }

        public CommandStatus OperateG41v2(short value, ushort index, OperateType opType, DatabaseHandle database)
        {
            return OperateAnalogOutput(value, index, database);
        }

        public CommandStatus SelectG41v3(float value, ushort index, DatabaseHandle database)
        {
            return SelectAnalogOutput(index);
        }

        public CommandStatus OperateG41v3(float value, ushort index, OperateType opType, DatabaseHandle database)
        {
            return OperateAnalogOutput(value, index, database);
        }

        public CommandStatus SelectG41v4(double value, ushort index, DatabaseHandle database)
        {
            return SelectAnalogOutput(index);
        }

        public CommandStatus OperateG41v4(double value, ushort index, OperateType opType, DatabaseHandle database)
        {
            return OperateAnalogOutput(value, index, database);
        }

        private CommandStatus SelectAnalogOutput(ushort index)
        {
            return index < 10 ? CommandStatus.Success : CommandStatus.NotSupported;
        }

        private CommandStatus OperateAnalogOutput(double value, ushort index, DatabaseHandle database)
        {
            if (index < 10)
            {
                database.Transaction(db =>
                    db.UpdateAnalogOutputStatus(new AnalogOutputStatus(index, value, new Flags(Flag.Online), Utility.Now()), UpdateOptions.DetectEvent())
                );
                return CommandStatus.Success;
            }
            else
            {
                return CommandStatus.NotSupported;
            }
        }
    }
    // ANCHOR_END: control_handler

    class TestConnectionStateListener : IConnectionStateListener
    {
        public void OnChange(ConnectionState state)
        {
            Console.WriteLine("Connection state change: " + state);
        }
    }

    class TestPortStateListener : IPortStateListener
    {
        public void OnChange(PortState state)
        {
            Console.WriteLine("Port state change: " + state);
        }
    }

    static class Utility
    {
        public static Timestamp Now()
        {
            return Timestamp.SynchronizedTimestamp((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        public static Dictionary<int, T> ToIntIndex<T>(this Dictionary<string, T> source)
        {
            var result = new Dictionary<int, T>();
            foreach (var kvp in source)
            {
                if (int.TryParse(kvp.Key, out int key))
                {
                    result[key] = kvp.Value;
                }
            }
            return result;
        }
        
        public static Dictionary<ushort, T> ToUShortIndex<T>(this Dictionary<string, T> source)
        {
            var result = new Dictionary<ushort, T>();
            foreach (var kvp in source)
            {
                if (ushort.TryParse(kvp.Key, out ushort key))
                {
                    result[key] = kvp.Value;
                }
            }
            return result;
        }
        
        public static Dictionary<ushort, bool> ToBinaryValues(this Dictionary<string, string> source)
        {
            var result = new Dictionary<ushort, bool>();
            foreach (var kvp in source)
            {
                if (ushort.TryParse(kvp.Key, out ushort key) && bool.TryParse(kvp.Value, out bool value))
                {
                    result[key] = value;
                }
            }
            return result;
        }
    }
}