import os
import logging
import asyncio
import random
from datetime import datetime, timezone, timedelta
from openleadr import OpenADRServer, enable_default_logging
from functools import partial

DEBUG = os.environ.get('DEBUG', '0')
enable_default_logging(logging.INFO if DEBUG == '0' else logging.DEBUG)

logger = logging.getLogger('openleadr')
for handler in logger.handlers:
    if handler.name == 'openleadr_default_handler':
        handler.setFormatter(logging.Formatter('%(asctime)s\t%(name)s\t%(levelname)s\t%(message)s'))


class Registration:
    def __init__(self, ven_id, registration_id, ven_name):
        self.ven_id = ven_id
        self.registration_id = registration_id
        self.ven_name = ven_name


class Map:
    def __init__(self):
        """Initialize an empty map."""
        self._data = {}

    def add(self, key, value):
        """Add a key-value pair to the map."""
        self._data[key] = value

    def get(self, key):
        """Retrieve the value associated with a key."""
        return self._data.get(key, None)

    def remove(self, key):
        """Remove a key-value pair from the map."""
        if key in self._data:
            del self._data[key]

    def contains(self, key):
        """Check if a key exists in the map."""
        return key in self._data

    def size(self):
        """Return the number of key-value pairs in the map."""
        return len(self._data)

    def keys(self):
        """Return a list of all keys in the map."""
        return list(self._data.keys())

    def values(self):
        """Return a list of all values in the map."""
        return list(self._data.values())

    def items(self):
        """Return a list of all key-value pairs in the map."""
        return list(self._data.items())


registered_clients = Map()


async def on_create_party_registration(registration_info):
    """
    Inspect the registration info and return a ven_id and registration_id.
    """
    ven_name = registration_info['ven_name']
    if ven_name:
        ven_id = 'vent_id_' + str(registration_info['ven_name']).lower()
        registration_id = 'reg_id_' + str(registration_info['ven_name']).lower()
        registration = Registration(ven_id, registration_id, ven_name)
        registered_clients.add(registration_id, registration)
        logger.info(
            f"Registered VEN {ven_name} with ven_id {ven_id}, registration_id: {registration_id}")
        return ven_id, registration_id
    else:
        logger.error('Invalid ven_name')
        return False


async def on_register_report(ven_id, resource_id, measurement, unit, scale,
                             min_sampling_interval, max_sampling_interval):
    """
    Inspect a report offering from the VEN and return a callback and sampling interval for receiving the reports.
    """
    callback = partial(on_update_report, ven_id=ven_id, resource_id=resource_id, measurement=measurement)
    sampling_interval = min_sampling_interval
    return callback, sampling_interval


async def on_update_report(data, ven_id, resource_id, measurement):
    """
    Callback that receives report data from the VEN and handles it.
    """
    for time, value in data:
        logger.debug(f"Ven {ven_id} reported {measurement} = {value} at time {time} for resource {resource_id}")


async def event_response_callback(ven_id, event_id, opt_type):
    """
    Callback that receives the response from a VEN to an Event.
    """
    logger.debug(f"VEN {ven_id} responded to Event {event_id} with: {opt_type}")


async def add_event_task(server):
    for registration in registered_clients.values():
        ven_id = registration.ven_id
        registration_id = registration.registration_id
        logger.debug(f'Generate event for client: {ven_id} {registration_id}')
        server.add_event(ven_id=ven_id,
                         signal_name='simple',
                         signal_type='level',
                         intervals=[{
                             'dtstart':
                                 datetime.now(timezone.utc) + timedelta(minutes=5),
                             'duration':
                                 timedelta(minutes=1),
                             'signal_payload':
                                 random.uniform(1.0, 100.0)
                         }],
                         callback=event_response_callback)
        logger.debug(f"Add event for VEN {ven_id}")


async def run_periodically(interval, server):
    while True:
        await add_event_task(server)
        logger.debug(f"Waiting for {interval} seconds...")
        await asyncio.sleep(interval)


async def run_event_task(server):
    # Run the periodic task with a 5-minute interval (300 seconds)
    await run_periodically(10, server)


def main():
    # Create the server object
    server = OpenADRServer(vtn_id='VTN', http_host='0.0.0.0')

    # Add the handler for client (VEN) registrations
    server.add_handler('on_create_party_registration', on_create_party_registration)

    # Add the handler for report registrations from the VEN
    server.add_handler('on_register_report', on_register_report)

    # Run the server on the asyncio event loop
    loop = asyncio.get_event_loop()
    loop.create_task(run_event_task(server))
    loop.create_task(server.run())
    loop.run_forever()


if __name__ == '__main__':
    main()
