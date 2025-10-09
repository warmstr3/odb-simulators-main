FROM python:3.9

# Copy SSL certificates to the container
COPY gridapi-cm-dev-appgw.dpit.nationalgrid.crt /usr/local/share/ca-certificates

# Update SSL certificates in the container
RUN update-ca-certificates

RUN mkdir -p /usr/src/app

COPY openleadr-python-main.zip /tmp/

# Use RUN to unzip the archive
RUN unzip /tmp/openleadr-python-main.zip -d /usr/src/app

# Optionally, remove the zip file to save space
RUN rm /tmp/openleadr-python-main.zip

COPY ven.py /usr/src/app/openleadr-python-main

WORKDIR /usr/src/app/openleadr-python-main

RUN pip install pyOpenSSL==24.2.1 --no-cache-dir -e .
RUN pip install pyOpenSSL==24.2.1 --no-cache-dir -r dev_requirements.txt

CMD [ "python", "ven.py" ]