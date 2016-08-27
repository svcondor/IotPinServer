Windows 10 IoT PinServer Sample
==============
IotPinServer creates a webserver on port  8800 of the Raspberry Pi. Any browser on the local nework can show and change the state of the GPIO pins.   Useful for checking that your breadboard is wired correctly before testing your own app.
 
![ScreenShot] (IotPinServer/IotPinServer.jpg?raw=true "Title"))

<ol>
    <li>https://github.com/tomkuijsten/restup is used to create the webserver</li>
    <li>Server code is a headless C# app. Client code is a JavaScript Single Page App</li>
</ol>

###Requirements:
<ol>
    <li>Raspberry Pi 2 or 3</li>
    <li>Windows 10 Core (Build 14393) installed and running on the Raspberry Pi</li>
    <li>Visual Studio 2015 Update 3(any version)</li>
    <li>Nuget package restup</li>
    <li> Update Nuget package Microsoft.NETCore.UniversalWindowsPlatform to 5.2.2</li>
</ol>

###Usage
In startup.cs task change port from 8800 if required. Build the solution and deploy to your Raspberry Pi.
The very first build will take a while and you'll likely get "Operation is taking longer than expected."   Once the app is up and running, browse to http://your-rpi-ip:8800. The driveMode and High/Low of all available pins is displayed and can be changed.   Pins in use by other apps are greyed out.

You can use IoT AppManager to set IotPinServer as "Add to Startup". 
