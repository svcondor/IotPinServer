using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using System.Diagnostics;
using Devkoes.Restup.WebServer.File;
using Devkoes.Restup.WebServer.Http;
using Devkoes.Restup.WebServer.Rest;

using Devkoes.Restup.WebServer.Attributes;
using Devkoes.Restup.WebServer.Models.Schemas;
using Devkoes.Restup.WebServer.Rest.Models.Contracts;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Foundation;
using System.Diagnostics.Tracing;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

//  Webservice for Universal Windows Apps
//  https://github.com/tomkuijsten/restup

namespace IotPinServer
{
    public sealed class StartupTask : IBackgroundTask
    {
      BackgroundTaskDeferral deferral;

    public async void Run(IBackgroundTaskInstance taskInstance) {
      int port = 8800;
      deferral = taskInstance.GetDeferral();

      var restRouteHandler = new RestRouteHandler();
      restRouteHandler.RegisterController<ParameterController>();

      var httpServer = new HttpServer(port);
      httpServer.RegisterRoute(new StaticFileRouteHandler(@"Web"));
      httpServer.RegisterRoute("api", restRouteHandler);

      await httpServer.StartServerAsync();
      Debug.WriteLine($"IotPinServer has started on port {port}");
    }
  }

  [RestController(InstanceCreationType.Singleton)]
  public sealed class ParameterController
  {
    List<Pin> pinList;
    List<Pin> pinUpdates;

    [UriFormat("?cmd=open&pin={pin}&value={value1}")]
    public IGetResponse open1(int pin, bool value1) {
      try {
        Pin pin1 = pinList.FirstOrDefault(c => c.pinNumber == pin);
        if (pin1.open == 1 && value1 == false) {
          pin1.gpioPin.ValueChanged -= pin_ValueChanged;
          pin1.gpioPin.Dispose();
          pin1.gpioPin = null;
          pin1.open = 0;
        }
        else if (pin1.open == 0 && value1 == true) {
          pin1.open = 1;
        }
        updatePin(pin1);
        return new GetResponse(GetResponse.ResponseStatus.OK);
      }
      catch (Exception e) {
        var v1 = e;
        throw;
      }
    }


    [UriFormat("?cmd=setdrivemode&pin={pin3}&value={value4}")]
    public IGetResponse setDriveMode1(int pin3, int value4) {
      try {
        GpioPinDriveMode newDriveMode = (GpioPinDriveMode)value4;
        Pin pin1 = pinList.FirstOrDefault(c => c.pinNumber == pin3);
        if (pin1.open != 1) {
          pin1.open = 1;
          updatePin(pin1);
        }
        if (pin1.open == 1) {
          if (pin1.gpioPin.IsDriveModeSupported(newDriveMode)) {
            pin1.gpioPin.SetDriveMode(newDriveMode);
          }
          else {
            Debug.WriteLine($"Pin {pin1} {newDriveMode} Unsuported");
          }
        }
        updatePin(pin1);
        return new GetResponse(GetResponse.ResponseStatus.OK);

      }
      catch (Exception e) {
        var v1 = e;
        throw;
      }
    }

    [UriFormat("?cmd=setvalue&pin={pin5}&value={value5}")]
    public IGetResponse setValue1(int pin5, bool value5) {
      try {
        Pin pin1 = pinList.FirstOrDefault(c => c.pinNumber == pin5);
        if (pin1.open != 1) {
          pin1.open = 1;
          updatePin(pin1);
        }
        if (pin1.driveMode == GpioPinDriveMode.Input || pin1.driveMode == GpioPinDriveMode.InputPullDown || pin1.driveMode == GpioPinDriveMode.InputPullUp) {
          pin1.gpioPin.SetDriveMode(GpioPinDriveMode.Output);
          updatePin(pin1);
        }
        if (pin1.open == 1) {
          if (value5) pin1.gpioPin.Write(GpioPinValue.High);
          else pin1.gpioPin.Write(GpioPinValue.Low);
        }
        updatePin(pin1);
        return new GetResponse(GetResponse.ResponseStatus.OK);
      }
      catch (Exception e) {
        var v1 = e;
        throw;
      }
    }


    [UriFormat("?cmd=machinedata")]
    public IGetResponse setValue2() {
      Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation eas = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
      string[] machineData = new string[] { eas.FriendlyName, eas.SystemProductName };
      return new GetResponse(GetResponse.ResponseStatus.OK, machineData);
    }

    [UriFormat("?cmd=pinstatus&firstTime={firstTime}&rand={rand}")]
    public IAsyncOperation<IGetResponse> pinstatus(int firstTime, string rand) {
      try {
        Debug.WriteLine($"pinstatus? in {firstTime}");
        return pinstatusAsync(firstTime).AsAsyncOperation();
      }
      catch (Exception e) {
        var v1 = e;
        throw;
      }
    }


    private async Task<IGetResponse> pinstatusAsync(int firstTime) {
      try {
        if (firstTime == 1) {
          initPinList();
        }
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while (pinUpdates.Count == 0 && stopwatch.ElapsedMilliseconds < 10000) {
          await Task.Delay(200);
        }
        List<Pin> pinUpdates2 = pinUpdates;
        pinUpdates = new List<Pin>();
        GetResponse v1;
        v1 = new GetResponse(GetResponse.ResponseStatus.OK, pinUpdates2);
        Debug.WriteLine($"pinstatus - {pinUpdates2.Count} pins");
        return v1;
      }
      catch (Exception e) {
        var v1 = e;
        return null;
      }
    }

    private void initPinList() {
      if (pinList != null) {
        foreach (Pin pin in pinList) {
          pinUpdates.Add(pin);
        }
      }
      else {
        pinList = new List<Pin>();
        pinUpdates = new List<Pin>();
        int pinCount = GpioController.GetDefault().PinCount;
        for (int pinIx = 0; pinIx < pinCount; ++pinIx) {
          GpioOpenStatus gpioOpenStatus;
          GpioPin gpioPin;

          GpioController.GetDefault().TryOpenPin(pinIx, GpioSharingMode.Exclusive, out gpioPin, out gpioOpenStatus);
          if (gpioOpenStatus == GpioOpenStatus.PinUnavailable) continue;
          Pin pin1 = new Pin();

          pinList.Add(pin1);
          pin1.pinNumber = pinIx;
          if (gpioOpenStatus == GpioOpenStatus.SharingViolation) {
            pin1.open = 2;
            if (gpioPin != null) {
              gpioPin.Dispose();
            }
          }
          else {
            pin1.driveMode = gpioPin.GetDriveMode();
            pin1.value = gpioPin.Read();
            gpioPin.Dispose();
          }
          pinUpdates.Add(pin1);
        }
      }
    }

    private void pin_ValueChanged(GpioPin gpioPin, GpioPinValueChangedEventArgs args) {
      var v1 = args;
      Debug.WriteLine($"Pin {gpioPin.PinNumber}   Edge {args.Edge}");
      Pin pin = pinList.FirstOrDefault(c => c.pinNumber == gpioPin.PinNumber);
      updatePin(pin);
    }

    private void updatePin(Pin pin) {
      GpioPin gpioPin;
      GpioOpenStatus os1;

      //Pin1 pin1 = new Pin1();

      if (pin.gpioPin == null) {
        GpioController.GetDefault().TryOpenPin(pin.pinNumber, GpioSharingMode.Exclusive, out gpioPin, out os1);

      }
      else {
        gpioPin = pin.gpioPin;
      }
      if (gpioPin == null) {
        pin.driveMode = GpioPinDriveMode.Input;
        pin.value = GpioPinValue.Low;
        pin.open = 2;
        pin.gpioPin = null;
      }
      else {
        pin.driveMode = gpioPin.GetDriveMode();
        pin.value = gpioPin.Read();
        if (pin.open == 1) {
          if (pin.gpioPin == null) {
            gpioPin.ValueChanged += pin_ValueChanged;
          }
          pin.gpioPin = gpioPin;

        }
        else {
          pin.gpioPin = null;
        }
      }

      if (pin.gpioPin == null && gpioPin != null) {
        gpioPin.Dispose();
      }
      pinUpdates.Add(pin);
    }
  }
  public sealed class Pin
  {
    public int open { get; set; }
    public int pinNumber { get; set; }
    public GpioPinDriveMode driveMode { get; set; }
    public GpioPinValue value { get; set; }
    [JsonIgnore]
    public GpioPin gpioPin { get; set; }
    public override string ToString() => $"{pinNumber} Open-{open} {driveMode} {value} {gpioPin == null}";

  }

}
