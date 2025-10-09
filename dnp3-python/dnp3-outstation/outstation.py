"""
DNP3 Outstation Simulator
=========================

Developed by DC&I - Jon Malaver
Email: jonatan.malaver@nationalgrid.com
All rights reserved. Unauthorized use, modification, or distribution is prohibited.

Overview:
---------
This Python script simulates multiple DNP3 outstations using the `pydnp3` library.
It supports two device types: SEL651R2 and SEL734P, with modular support for future expansion.

Features:
---------
- Predefined analog, binary, and binary output status values per device type.
- Custom command handling logic for DNP3 control commands.
- Periodic analog signal simulation using sine wave functions.
- Dynamic configuration via command-line arguments and YAML-based IP assignment.

Usage:
------
Run the script with:
    python script_name.py --base_port 20000 --devices SEL651R2:2 SEL734P:3

Dependencies:
-------------
- Python 3.x
- pydnp3
- PyYAML

Required Files:
---------------
- `ips.yaml`: A YAML file containing a list of IP addresses under the key `ips`.
- IMPORTANT: The server must already be configured with these IP addresses.
  If the IPs are not configured on the host machine, the outstations will fail to bind

Extending:
----------
To add a new device type:
1. Create a subclass of `BaseOutstation`.
2. Define database sizes and event buffer config.
3. Add the device to `predefined_database` and `DEVICE_REGISTRY`.
4. OPTIONAL: Add to how to handle commands to SimpleCommandHandler Class

License:
--------
Proprietary � Internal use only.

"""
import os
import time
import threading
import math
import logging
import sys
from itertools import chain
from pydnp3 import opendnp3, asiodnp3, asiopal

DEBUG = os.environ.get('DEBUG', '0')
LOG_LEVELS = opendnp3.levels.NORMAL if DEBUG == '0' else opendnp3.levels.NORMAL | opendnp3.levels.ALL_COMMS

LOCAL_IP = os.environ.get('OUTSTATION_IP', '0.0.0.0')
PORT = os.environ.get('OUTSTATION_PORT', '20000')
OUTSTATION_ID = os.environ.get('OUTSTATION_ID', '10')
MASTER_ID = os.environ.get('MASTER_ID', '1')
DEVICE_TYPE = os.environ.get('DEVICE_TYPE', 'SEL651R2')

stdout_stream = logging.StreamHandler(sys.stdout)
stdout_stream.setFormatter(logging.Formatter('%(asctime)s\t%(name)s\t%(levelname)s\t%(message)s'))

_log = logging.getLogger(__name__)
_log.addHandler(stdout_stream)
_log.setLevel(logging.INFO if DEBUG == '0' else logging.DEBUG)

# Predefined DNP3 database
predefined_database = {
    'SEL651R2': {
        'Analog': {
            0: 125.132568359375, 1: 132.519775390625, 2: 136.558837890625, 3: 29.4130859375, 4: 7924.1708125000005,
            5: 3130.364061342821,
            6: 0.2502028294873032, 7: 0.0, 8: 0.0, 9: 0.0, 10: 0.0, 11: 125.81907907478198, 12: 133.1163793288955,
            13: 137.13357708368144, 14: 29.59809316500222, 15: 3143.5169501754767, 16: 7989.595562500001,
            17: 8008.628500000001, 18: 7988.0911875,
            19: 7920.00525, 20: 8006.014, 21: 990.818933811491, 22: 1048.59772427249, 23: 1090.94740325884,
            24: 133.3983674749391, 25: -56.324334765749924, 26: -76.82382987970188, 27: 999.7586128882905,
            28: 1050.1093362294466, 29: 1093.6490010577393, 30: 99.10581624788679, 31: 99.8560519457541,
            32: 99.75297396182079,
            33: 13755.435628539786, 34: 13787.398498189514, 35: 13892.022132171007, 36: 13749.950647179568,
            37: 13781.685903402382,
            38: 13888.876404387587
        },
        'Binary': {
            0: True, 1: False, 2: False, 3: False, 4: True, 5: True, 6: True, 7: False, 8: False, 9: False, 10: False,
            11: False,
            12: False, 13: False, 14: False, 15: False, 16: True, 17: False, 18: True, 19: False, 20: False, 21: False,
            22: False,
            23: False, 24: False, 25: False, 26: False, 27: False, 28: False, 29: False, 30: True, 31: False, 32: False,
            33: False,
            34: False, 35: False
        },
        'BinaryOutputStatus': {0: False, 1: False, 2: False, 3: False, 4: False, 5: False, 6: False, 7: False, 8: False}
    },
    'SEL734P': {
        'Analog': {
            0: 167.0, 1: 180.0, 2: 154.0, 3: 8291.0, 4: 8182.0, 5: 8295.0, 6: 12.0, 7: 4100.0, 8: 681.0,
            9: 4146.0, 10: 0.0, 11: 1392.0, 12: 1468.0, 13: 1316.0, 14: 186.0, 15: 226.0, 16: 260.0, 17: 12419.0,
            18: 12323.0,
            19: 12492.0, 20: -7.0, 21: -128.0, 22: 109.0, 23: -81.0, 24: 0.0, 25: 240.0,
            26: 120.0
        },
        'Binary': {0: True, 1: False, 2: False, 3: False, 4: False, 5: False},
        'BinaryOutputStatus': {0: False}
    }
}


class SimpleCommandHandler(opendnp3.ICommandHandler):
    def __init__(self, outstation, device_type):
        super().__init__()
        self.outstation = outstation
        self.device_type = device_type

    def Start(self):
        return opendnp3.CommandStatus.SUCCESS

    def End(self):
        pass

    def Select(self, command, index):
        return opendnp3.CommandStatus.SUCCESS

    def Operate(self, command, index, op_type):
        value = self._interpret_control_code(command.functionCode)
        self._handle_command(index, value)
        return opendnp3.CommandStatus.SUCCESS

    def DirectOperate(self, command, index):
        value = self._interpret_control_code(command.functionCode)
        self._handle_command(index, value)
        return opendnp3.CommandStatus.SUCCESS

    def _interpret_control_code(self, control_code):
        if control_code in (opendnp3.ControlCode.CLOSE_PULSE_ON, opendnp3.ControlCode.LATCH_ON):
            return True
        elif control_code in (opendnp3.ControlCode.TRIP_PULSE_ON, opendnp3.ControlCode.LATCH_OFF):
            return False
        else:
            return False

    def _handle_command(self, index, value):
        update = asiodnp3.UpdateBuilder()

        if self.device_type == "SEL651R2":
            # Existing logic for SEL651R2
            match index:
                case 0:
                    update.Update(opendnp3.Binary(value), 0, opendnp3.EventMode.Detect)
                    update.Update(opendnp3.Binary(not value), 1, opendnp3.EventMode.Detect)
                case 1:
                    update.Update(opendnp3.Binary(value), 4, opendnp3.EventMode.Detect)
                case 2:
                    update.Update(opendnp3.Binary(value), 5, opendnp3.EventMode.Detect)
                case 3:
                    update.Update(opendnp3.Binary(value), 6, opendnp3.EventMode.Detect)
                    update.Update(opendnp3.Binary(not value), 7, opendnp3.EventMode.Detect)
                case 4:
                    update.Update(opendnp3.Binary(value), 7, opendnp3.EventMode.Detect)
                    update.Update(opendnp3.Binary(not value), 6, opendnp3.EventMode.Detect)
                case 5:
                    update.Update(opendnp3.Binary(value), 8, opendnp3.EventMode.Detect)
                case 6:
                    update.Update(opendnp3.Binary(value), 9, opendnp3.EventMode.Detect)
                case 7 if value:
                    for bi_index in chain(range(12, 16), [17], range(19, 21), range(24, 27)):
                        update.Update(opendnp3.Binary(not value), bi_index, opendnp3.EventMode.Detect)
                case 8 if value:
                    for ai_index in range(11, 15):
                        update.Update(opendnp3.Analog(0.0), ai_index, opendnp3.EventMode.Detect)

            # Update Binary Output Status (Group 10/2)
            if index < 9:
                update.Update(opendnp3.BinaryOutputStatus(value), index, opendnp3.EventMode.Detect)

        elif self.device_type == "SEL734P":
            # Logic for SEL734P
            if index == 0:
                update.Update(opendnp3.Binary(value), 0, opendnp3.EventMode.Detect)
                update.Update(opendnp3.BinaryOutputStatus(value), 0, opendnp3.EventMode.Detect)

        self.outstation.Apply(update.Build())


class SimpleOutstationApplication(opendnp3.IOutstationApplication):
    def __init__(self): super().__init__()


class BaseOutstation:
    def __init__(self, device_type, local_addr, port, ip_address, remote_addr=1):
        self.device_type = device_type
        self.log_handler = asiodnp3.ConsoleLogger().Create()
        self.manager = asiodnp3.DNP3Manager(1, self.log_handler)
        retry = asiopal.ChannelRetry()
        log_levels = LOG_LEVELS

        self.channel = self.manager.AddTCPServer(
            f"server_{local_addr}",
            log_levels,
            retry,
            ip_address,
            port,
            asiodnp3.PrintingChannelListener()
        )

        db_sizes = self.get_database_sizes()
        self.config = asiodnp3.OutstationStackConfig(db_sizes)
        self.config.outstation.eventBufferConfig = self.get_event_buffer_config()
        self.config.link.LocalAddr = local_addr
        self.config.link.RemoteAddr = remote_addr
        self.config.outstation.params.allowUnsolicited = True
        self.config.outstation.params.typesAllowedInClass0 = opendnp3.StaticTypeBitField.AllTypes()

        self.command_handler = SimpleCommandHandler(None, device_type)
        self.outstation = self.channel.AddOutstation(
            f"{device_type}_{local_addr}",
            self.command_handler,
            SimpleOutstationApplication(),
            self.config
        )
        self.command_handler.outstation = self.outstation

        self.outstation.Enable()
        time.sleep(0.5)
        self.initialize_database()
        self.start_analog_simulation()

    def get_database_sizes(self):
        raise NotImplementedError("Subclasses must implement get_database_sizes")

    def get_event_buffer_config(self):
        raise NotImplementedError("Subclasses must implement get_event_buffer_config")

    def initialize_database(self):
        update = asiodnp3.UpdateBuilder()
        db = predefined_database[self.device_type]

        for index, value in db.get('Binary', {}).items():
            update.Update(opendnp3.Binary(value), index, opendnp3.EventMode.Detect)
        for index, value in db.get('Analog', {}).items():
            update.Update(opendnp3.Analog(value), index, opendnp3.EventMode.Detect)
        for index, value in db.get('BinaryOutputStatus', {}).items():
            update.Update(opendnp3.BinaryOutputStatus(value), index, opendnp3.EventMode.Detect)

        self.outstation.Apply(update.Build())

    def shutdown(self):
        self.manager.Shutdown()


class SEL651R2Outstation(BaseOutstation):
    def __init__(self, local_addr, port, ip_address, remote_addr=1):
        super().__init__('SEL651R2', local_addr, port, ip_address, remote_addr)

    def get_database_sizes(self):
        return opendnp3.DatabaseSizes(36, 0, 33, 9, 0, 0, 0, 0)

    def get_event_buffer_config(self):
        return opendnp3.EventBufferConfig(
            maxBinaryEvents=10,
            maxAnalogEvents=10,
            maxBinaryOutputStatusEvents=10
        )

    def start_analog_simulation(self, interval=5):
        def simulate():
            t = 0
            while True:
                update = asiodnp3.UpdateBuilder()
                for i in range(3):
                    value = 130.0 + 10.0 * math.sin(t + i)
                    update.Update(opendnp3.Analog(value), i, opendnp3.EventMode.Detect)
                self.outstation.Apply(update.Build())
                t += 0.5
                time.sleep(interval)

        thread = threading.Thread(target=simulate, daemon=True)
        thread.start()


class SEL734POutstation(BaseOutstation):
    def __init__(self, local_addr, port, ip_address, remote_addr=1):
        super().__init__('SEL734P', local_addr, port, ip_address, remote_addr)

    def get_database_sizes(self):
        return opendnp3.DatabaseSizes(6, 0, 27, 1, 0, 0, 0, 0)

    def get_event_buffer_config(self):
        return opendnp3.EventBufferConfig(
            maxBinaryEvents=5,
            maxAnalogEvents=27,
            maxBinaryOutputStatusEvents=1
        )

    def start_analog_simulation(self, interval=5):
        def simulate():
            t = 0
            while True:
                update = asiodnp3.UpdateBuilder()
                for i in range(3):
                    value = 160.0 + 10.0 * math.sin(t + i)
                    update.Update(opendnp3.Analog(value), i, opendnp3.EventMode.Detect)
                self.outstation.Apply(update.Build())
                t += 0.5
                time.sleep(interval)

        thread = threading.Thread(target=simulate, daemon=True)
        thread.start()


DEVICE_REGISTRY = {
    "SEL651R2": SEL651R2Outstation,
    "SEL734P": SEL734POutstation,
    # Add future devices here
}


def main():

    device_class = DEVICE_REGISTRY.get(DEVICE_TYPE)
    if not device_class:
        _log.error(f"Unknown device type: {DEVICE_TYPE}")
    else:
        outstation = device_class(local_addr=int(OUTSTATION_ID), port=int(PORT), ip_address=LOCAL_IP, remote_addr=int(MASTER_ID))
        _log.info(f"{DEVICE_TYPE} Outstation started LocalAddr={OUTSTATION_ID}, Port={PORT}, IP={LOCAL_IP}")
        try:
            while True:
                time.sleep(1)
        except KeyboardInterrupt:
            _log.info("Shutting down...")
            outstation.shutdown()


if __name__ == "__main__":
    main()
