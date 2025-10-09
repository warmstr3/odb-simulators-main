### Endpoints

Run form the VM

```shell

curl --cert ~/tls/certs/dev1.crt --key ~/tls/private/dev1.pem -v -k  https://10.15.80.251/dcap

<DeviceCapability xmlns="urn:ieee:std:2030.5:ns" href="/dcap" pollRate="900">
  <DERProgramListLink href="/derp" all="0"/>
  <TimeLink href="/tm"/>
  <UsagePointListLink href="/upt" all="0"/>
  <EndDeviceListLink href="/edev" all="1"/>
  <MirrorUsagePointListLink href="/mup" all="0"/>
  <SelfDeviceLink href="/sdev"/>
</DeviceCapability>

curl --cert ~/tls/certs/dev1.crt --key ~/tls/private/dev1.pem -v -k  https://10.15.80.251/edev

<EndDeviceList xmlns="urn:ieee:std:2030.5:ns" href="/edev" subscribable="0" all="1" results="1" pollRate="900">
  <EndDevice href="/edev_0" subscribable="0">
    <ConfigurationLink href="/edev_0_cfg"/>
    <DERListLink href="/edev_0_der" all="0"/>
    <DeviceInformationLink href="/edev_0_di"/>
    <DeviceStatusLink href="/edev_0_dstat"/>
    <lFDI>1C71430E4A75616F25501FC3B2C9BD5C7089079A</lFDI>
    <LogEventListLink href="/edev_0_lel" all="0"/>
    <PowerStatusLink href="/edev_0_ps"/>
    <sFDI>76349565168</sFDI>
    <changedTime>1749057374</changedTime>
    <enabled>true</enabled>
    <FunctionSetAssignmentsListLink href="/edev_0_fsa" all="2"/>
    <postRate>900</postRate>
    <RegistrationLink href="/edev_0_rg"/>
  </EndDevice>
</EndDeviceList>

 curl --cert ~/tls/certs/dev1.crt --key ~/tls/private/dev1.pem -v -k  https://10.15.80.251/edev_0_der

<DERList xmlns="urn:ieee:std:2030.5:ns" href="/edev_0_der" all="1" results="1" pollRate="900">
  <DER href="/der_0" subscribable="0">
    <CurrentDERProgramLink href="/derp_0"/>
    <DERAvailabilityLink href="/der_0_dera"/>
    <DERCapabilityLink href="/der_0_dercap"/>
    <DERSettingsLink href="/der_0_derg"/>
    <DERStatusLink href="/der_0_ders"/>
  </DER>
</DERList>

curl --cert ~/tls/certs/dev1.crt --key ~/tls/private/dev1.pem -v -k  https://10.15.80.251/derp0

<DERProgramList xmlns="urn:ieee:std:2030.5:ns" href="/derp" subscribable="0" all="1" results="1" pollRate="900">
  <DERProgram href="/derp_0" subscribable="0">
    <description>Program 1</description>
    <ActiveDERControlListLink href="/derp_0_derca" all="0"/>
    <DefaultDERControlLink href="/derp_0_dderc"/>
    <DERControlListLink href="/derp_0_derc" all="0"/>
    <DERCurveListLink href="/derp_0_dc" all="0"/>
    <primacy>0</primacy>
  </DERProgram>
</DERProgramList>

curl --cert ~/tls/certs/dev1.crt --key ~/tls/private/dev1.pem -v -k  https://10.15.80.251/derp_0_dderc

<DefaultDERControl xmlns="urn:ieee:std:2030.5:ns" href="/derp_0_dderc" subscribable="0">
  <DERControlBase>
    <opModConnect>true</opModConnect>
    <opModEnergize>true</opModEnergize>
  </DERControlBase>
  <setESDelay>30</setESDelay>
  <setGradW>1000</setGradW>
</DefaultDERControl>

```