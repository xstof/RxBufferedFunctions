# need to run from cmdline: docker run --rm --add-host host.docker.internal:host-gateway -p 8089:8089 -v $PWD:/mnt/locust locustio/locust -f /mnt/locust/locustfile.py
import time
from locust import HttpUser, task, between

class SmokeTestUser(HttpUser):
    wait_time = between(1, 1)

    @task
    def hello_world(self):
        headers = {'content-type': 'application/json'}
        payload = {"DeviceId": "device-01","Channel": "BBCOne"}
        # self.client.post("https://enwqssqhcrx1.x.pipedream.net", json=payload)
        payload = {"DeviceId": "device-01","Channel": "BBCOne"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

        payload = {"DeviceId": "device-02","Channel": "BBCOne"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

        payload = {"DeviceId": "device-03","Channel": "BBCOne"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

        payload = {"DeviceId": "device-04","Channel": "BBCTwo"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

        payload = {"DeviceId": "device-05","Channel": "BBCTwo"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

class AlwaysConnectedUserBBCOne(HttpUser):
    weight = 4
    wait_time = between (1,1)

    @task(1)
    def sendTelemetry1(self):
        payload = {"DeviceId": "device-01","Channel": "BBCOne"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

    @task(1)
    def sendTelemetry2(self):
        payload = {"DeviceId": "device-02","Channel": "BBCOne"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

    @task(1)
    def sendTelemetry3(self):
        payload = {"DeviceId": "device-03","Channel": "BBCOne"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

class AlwaysConnectedUserBBCTwo(HttpUser):
    weight = 4
    wait_time = between (1,1)

    @task(1)
    def sendTelemetry1(self):
        payload = {"DeviceId": "device-04","Channel": "BBCTwo"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

    @task(1)
    def sendTelemetry2(self):
        payload = {"DeviceId": "device-05","Channel": "BBCTwo"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

    @task(1)
    def sendTelemetry3(self):
        payload = {"DeviceId": "device-06","Channel": "BBCTwo"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

class OcassionallyConnectedUserBBCOne(HttpUser):
    weight = 1
    wait_time = between (7,12)

    @task(1)
    def sendTelemetry1(self):
        payload = {"DeviceId": "device-07-occasional","Channel": "BBCOne"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

class OcassionallyConnectedUserBBCTwo(HttpUser):
    weight = 1
    wait_time = between (7,12)
    
    @task(1)
    def sendTelemetry1(self):
        payload = {"DeviceId": "device-08-occasional","Channel": "BBCTwo"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

    @task(1)
    def sendTelemetry2(self):
        payload = {"DeviceId": "device-09-occasional","Channel": "BBCTwo"}
        self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

        # self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", data={
        #     "Channel": "BBC_One",
        #     "SessionId": "aaaa-aaaa-aaaa-0000",
        #     "DeviceId": "abfa-aaaa-ab4a-0001",
        #     "eventClass": 0,
        #     "HappenedAt": "2021-06-14T00:00:00"
        # })
        # self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", data={
        #     "Channel": "BBC_Two",
        #     "SessionId": "aaaa-aaaa-aaaa-0000",
        #     "DeviceId": "abfa-aaaa-ab4a-0002",
        #     "eventClass": 0,
        #     "HappenedAt": "2021-06-14T00:00:00"
        # })
        # self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", data={
        #     "Channel": "Sky_Sports",
        #     "SessionId": "aaaa-aaaa-aaaa-0000",
        #     "DeviceId": "abfa-aaaa-ab4a-0003",
        #     "eventClass": 0,
        #     "HappenedAt": "2021-06-14T00:00:00"
        # })