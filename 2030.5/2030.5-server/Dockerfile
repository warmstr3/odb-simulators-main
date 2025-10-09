FROM python:3.10

RUN mkdir -p /usr/src/app

COPY gridappsd-2030_5-0.0.2a14.zip /tmp/

# Use RUN to unzip the archive
RUN unzip /tmp/gridappsd-2030_5-0.0.2a14.zip -d /usr/src/app

# Optionally, remove the zip file to save space
RUN rm /tmp/gridappsd-2030_5-0.0.2a14.zip

COPY config.yml /usr/src/app/gridappsd-2030_5-0.0.2a14
COPY openssl.cnf /usr/src/app/gridappsd-2030_5-0.0.2a14
COPY pyproject.toml /usr/src/app/gridappsd-2030_5-0.0.2a14

#COPY patches/ieee_2030_5/certs.py /usr/src/app/gridappsd-2030_5-0.0.2a14/ieee_2030_5

WORKDIR /usr/src/app/gridappsd-2030_5-0.0.2a14

# Create a virtual environment
RUN python3 -m venv venv

# Set environment variables
ENV VIRTUAL_ENV=/usr/src/app/gridappsd-2030_5-0.0.2a14/venv
ENV PATH="$VIRTUAL_ENV/bin:$PATH"

RUN pip install poetry
RUN poetry install

RUN adduser --disabled-password --gecos "" volttron

RUN chown volttron:volttron /usr/src/app/gridappsd-2030_5-0.0.2a14

USER volttron

#RUN mkdir -p ~/tls/certs
#RUN openssl req -x509 -newkey rsa:4096 -keyout ~/tls/certs/dev1.pem -out ~/tls/certs/dev1.crt -days 365 -nodes -subj '/CN=issuer'
#RUN openssl req -x509 -newkey rsa:4096 -keyout ~/tls/certs/dev2.pem -out ~/tls/certs/dev2.crt -days 365 -nodes -subj '/CN=issuer'

# CMD [ "python", "-m", "ieee_2030_5", "--debug", "config.yml" ]
CMD [ "python", "-m", "ieee_2030_5", "--no-validate", "--debug", "config.yml" ]