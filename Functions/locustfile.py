# need to run from cmdline: docker run --rm --add-host host.docker.internal:host-gateway -p 8089:8089 -v $PWD:/mnt/locust locustio/locust -f /mnt/locust/locustfile.py SmokeTestUser
# or: docker run --rm --add-host host.docker.internal:host-gateway -p 8089:8089 -v $PWD:/mnt/locust locustio/locust -f /mnt/locust/locustfile.py AlwaysConnectedUserBBCOne AlwaysConnectedUserBBCTwo OcassionallyConnectedUserBBCOne OcassionallyConnectedUserBBCTwo

# configure host with value "http://host.docker.internal:7071" when running within local container
# to run this at scale, please check: https://github.com/yorek/locust-on-azure

import time, uuid
from locust import HttpUser, task, between

class AlwaysConnectedUserBBCOne(HttpUser):
    weight = 49
    wait_time = between (1,1)
    devicename= ""

    def on_start(self):
        self.devicename = "device-bbcone-" + str(uuid.uuid1())

    @task(1)
    def sendTelemetry1(self):
        payload = {"DeviceId": self.devicename,"Channel": "BBCOne"}
        self.client.post("/api/ReceiveRaw", json=payload)

    # @task(1)
    # def sendTelemetry2(self):
    #     payload = {"DeviceId": "device-02","Channel": "BBCOne"}
    #     self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

    # @task(1)
    # def sendTelemetry3(self):
    #     payload = {"DeviceId": "device-03","Channel": "BBCOne"}
    #     self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

class AlwaysConnectedUserBBCTwo(HttpUser):
    weight = 49
    wait_time = between (1,1)
    devicename= ""

    def on_start(self):
        self.devicename = "device-bbctwo-" + str(uuid.uuid1())

    @task(1)
    def sendTelemetry1(self):
        payload = {"DeviceId": self.devicename,"Channel": "BBCTwo"}
        self.client.post("/api/ReceiveRaw", json=payload)

    # @task(1)
    # def sendTelemetry2(self):
    #     payload = {"DeviceId": "device-05","Channel": "BBCTwo"}
    #     self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

    # @task(1)
    # def sendTelemetry3(self):
    #     payload = {"DeviceId": "device-06","Channel": "BBCTwo"}
    #     self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

class OcassionallyConnectedUserBBCOne(HttpUser):
    weight = 1
    wait_time = between (7,15)
    devicename= ""

    def on_start(self):
        self.devicename = "device-occasional-bbcone-" + str(uuid.uuid1())

    @task(1)
    def sendTelemetry1(self):
        payload = {"DeviceId": self.devicename,"Channel": "BBCOne"}
        self.client.post("/api/ReceiveRaw", json=payload)

class OcassionallyConnectedUserBBCTwo(HttpUser):
    weight = 1
    wait_time = between (7,15)
    devicename= ""

    def on_start(self):
        self.devicename = "device-occasional-bbctwo-" + str(uuid.uuid1())

    @task(1)
    def sendTelemetry1(self):
        payload = {"DeviceId": self.devicename,"Channel": "BBCTwo"}
        self.client.post("/api/ReceiveRaw", json=payload)

    # @task(1)
    # def sendTelemetry2(self):
    #     payload = {"DeviceId": "device-09-occasional","Channel": "BBCTwo"}
    #     self.client.post("http://host.docker.internal:7071/api/ReceiveRaw", json=payload)

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