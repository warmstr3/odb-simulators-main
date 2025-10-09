FROM python:3.10

RUN mkdir -p /usr/src/app/dnp3

WORKDIR /usr/src/app/dnp3

COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

COPY outstation.py .

CMD [ "python", "outstation.py" ]
EXPOSE 20000
