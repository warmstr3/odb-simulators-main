import os
import logging
import asyncio
from datetime import timedelta
from functools import partial
from random import random, randint
from openleadr import OpenADRClient, enable_default_logging

VTN = os.environ.get('VTN', 'http://localhost:8080/OpenADR2/Simple/2.0b')
DEBUG = os.environ.get('DEBUG', '0')
VEN_NAME = os.environ.get('VEN_NAME', 'ven123')

enable_default_logging(logging.INFO if DEBUG == '0' else logging.DEBUG)

logger = logging.getLogger('openleadr')
for handler in logger.handlers:
    if handler.name == 'openleadr_default_handler':
        handler.setFormatter(logging.Formatter('%(asctime)s\t%(name)s\t%(levelname)s\t%(message)s'))

device_report_data = [{
    'report_specifier_id': 'AmpereReport',
    'resource_id': 'Device001',
    'measurement': 'Current',
    'sampling_rate': 10,
    'unit': 'A'
}, {
    'report_specifier_id': 'AmpereReport',
    'resource_id': 'Device002',
    'measurement': 'Current',
    'sampling_rate': 10,
    'unit': 'A'
}, {
    'report_specifier_id': 'VoltageReport',
    'resource_id': 'Device001',
    'measurement': 'Voltage',
    'sampling_rate': 10,
    'unit': 'V'
}, {
    'report_specifier_id': 'VoltageReport',
    'resource_id': 'Device002',
    'measurement': 'Voltage',
    'sampling_rate': 10,
    'unit': 'V'
}, {
    'report_specifier_id': 'StatusReport',
    'resource_id': 'Device001'
}, {
    'report_specifier_id': 'StatusReport',
    'resource_id': 'Device002'
}]


async def read_voltage(device):
    """
    Retrieve the voltage value from the given 'device'.
    """
    return 100 * random()


async def read_current(device):
    """
    Retrieve the current value from the given 'device'.
    """
    return random()


async def collect_status():
    return randint(0, 1)


async def handle_event(event):
    # This callback receives an Event dict.
    # You should include code here that sends control signals to your resources.
    logger.debug('Event Received')
    logger.debug(event)
    return 'optIn'


def main():
    # Create the client object
    logger.info(f'Connection to vtn: {VTN}')

    client = OpenADRClient(ven_name=VEN_NAME,
                           vtn_url=VTN)

    for device_report in device_report_data:
        # Add the report capability to the client
        measurement = device_report.get('measurement', None)
        report_specifier_id = device_report['report_specifier_id']
        device = device_report['resource_id']
        sample_rate = device_report.get('sampling_rate', 10)
        unit = device_report.get('unit', None)
        if measurement == 'Current':
            client.add_report(callback=partial(read_current, device=device),
                              report_specifier_id=report_specifier_id,
                              resource_id=device,
                              measurement=measurement,
                              sampling_rate=timedelta(seconds=sample_rate),
                              unit=unit)
        elif measurement == 'Voltage':
            client.add_report(callback=partial(read_voltage, device=device),
                              report_specifier_id=report_specifier_id,
                              resource_id=device,
                              measurement=measurement,
                              sampling_rate=timedelta(seconds=sample_rate),
                              unit=unit)
        else:
            client.add_report(callback=collect_status,
                              report_name='TELEMETRY_STATUS',
                              report_specifier_id=report_specifier_id,
                              resource_id=device)

    # Add event handling capability to the client
    client.add_handler('on_event', handle_event)

    # Run the client in the Python AsyncIO Event Loop
    loop = asyncio.get_event_loop()
    loop.create_task(client.run())
    loop.run_forever()


if __name__ == '__main__':
    main()
