# Windows 10 IoT PinServer Sample

IotPinServer creates a website on the Raspberry Pi.   Any browser on the local network can show and change the state of the GPIO pins.   You can check your breadboard connections before testing your own app.   

Pins in use by other apps are greyed out. 

Some changes are automatic. If you toggle High/Low the pin will be opened and the mode will be changed to output.  

If you open an input pin it will be continuosly polled and any changes will be displayed.   

Selecting Input instead of PullUp or PullDown will give random High/Low readings if the pin is not connected.

The Raspberry Pi does not support OpenDrain or OpenSource optput modes.

![ScreenShot] (IotPinServer1.jpg)

<ul>
    <li>https://github.com/tomkuijsten/restup is used to create the webserver</li>
    <li>Server code is a headless C# app. Client code is a JavaScript Single Page App</li>
</ul>

### Requirements:
<ol>
    <li>Raspberry Pi 2 or 3</li>
    <li>Windows 10 Core (Build 14393) installed and running on the Raspberry Pi</li>
    <li>Visual Studio 2015 Update 3(Community or other)</li>
    <li>Nuget package restup</li>
    <li> Nuget package Microsoft.NETCore.UniversalWindowsPlatform to 5.2.2</li>
</ol>

### Usage
Either clone this repo or download the zip file.   Your Windows 10 PC needs to be in developer mode.

Open the solution in Visual Studio and build it.   Nuget packages will be installed or updated as necessary.

In Debug Properties select Remote Machine and select your Raspberry Pi as the target device.

Select Debug Start to deploy to your Raspberry Pi.   The first build/deploy will take a while and you'll likely get "Operation is taking longer than expected."

When the deploy has finished you'll see "IotPinServer has started on port 8800" in the output Debug window.   You can change this port number in startup.cs if required.

Browse to http://your-rpi-ip:8800        

You can use the IoT AppManager "Add to Startup". To make the app always available.

Currently using it in more than one browser concurrently will give unpredictable results. 
