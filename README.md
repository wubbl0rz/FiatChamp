# ![image](https://user-images.githubusercontent.com/30373916/190129327-ca33228f-9864-418a-a65c-8be4de9592bc.png)  FiatChamp 🚗 

Connect your FIAT (uconnect) car to Home Assistant. Needs a car with uconnect services enabled and valid fiat account.

I have created this addon for my own car (new Fiat Icon 500e) and its the only one i can test it with. 
Work in progress so expect some bugs. 😅

Example dashboard using sensors and entities provided by this addon:

![image](https://user-images.githubusercontent.com/30373916/190108698-6df2a4de-776d-45e2-8f27-1c5521f79476.png)

## Prerequisites

- Official Home Assistant MQTT Addon up and running. 

## Features

- Imports values like battery level, tyre pressure, odometet etc. into Home Assistant.
- Supports multiple cars on the same account. 
- Location tracking.
- Uses the same data source as the official app.
- Remote commands (open doors, switch air conditioner on, ...) are supported since version 2.0. Some commands may not work with all cars. Available commands are:
  - "UpdateLocation" (updates gps location from the car) 
  - "DeepRefresh" (gets battery charge level)
  - "Blink" (blink lights)
  - "ChargeNOW" (starts charging)
  - "Trunk" (open/close trunk lock)
  - "HVAC" (turn on the temperature preconditioning in the car. __the official app does not support turning preconditioning off 😅 i found an hidden command for this but i don't know if it will work or have negative side effects. enable it by setting the "EnableDangerousCommands" option.__)

## What doesn't work (yet)?

- Eco Reports (statistics). I could not find any API yet. The official app shows it so in theory it should be accessible.

## What will NEVER work?

- Things the fiat api does not support. Like real time tracking or adjusting the music volume. Maybe they add some new features in the future. 

## How to install

Follow the official docs:

https://www.home-assistant.io/addons/ 

Short version:

- Switch on "Advanced Mode" in your users profile. (if you haven't already)
- Go to Settings -> Add-ons -> ADD-ON STORE
- Top right three dots. Add repo. https://github.com/wubbl0rz/FiatChamp 
- Top right three dots. Check for updates.
- Refresh Page. (F5)
- Store should show this repo now and you can install the addon.

## Options / Usage

Fill out the required fields in the addon configuration. 

- PIN is only needed if you want to send commands to your car. Its the same PIN used by the official app or website.
- Use DEBUG carefully. It will dump many informations to the log including session tokens and sensitive informations.
- Automatic refresh of location and battery level may drain your battery a bit more. The car have to wakeup some parts, read new values and sent them back. This will get executed every "Refresh interval" and at every command even if your car is not at home. __Recommendation:__  Better use a Home Assistant automation instead. I have setup and automation that is triggered by plugin the charger cable and then updates the battery level (by calling DeepRefresh) every 30 minutes until its 100% or charger is disconnected.
- Only set "Dangerous commands" if you want to use unoffical commands that are not present in the the official app.

You dont have to configure MQTT. At startup the Addon will automatically connect to your Home Assistants MQTT Broker.

<img src="https://user-images.githubusercontent.com/30373916/193454721-8dcfa211-58a2-4d12-95f1-87db42beeec1.png" width="700px">
