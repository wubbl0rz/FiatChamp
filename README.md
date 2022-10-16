# ![image](https://user-images.githubusercontent.com/30373916/190129327-ca33228f-9864-418a-a65c-8be4de9592bc.png)  FiatChamp 🚗 

Connect your FIAT (uconnect) car to Home Assistant. Needs a car with uconnect services enabled and valid fiat account.

__Dodge or Jeep Uconnect Users: This addon may not work with your car. I do not own such a car and so i cannot test it. Some of the urls looks very "fiat only". If you want to help get this working for these cars too please open an issue. Some Jeep cars are reported working: https://github.com/wubbl0rz/FiatChamp/issues/11#issuecomment-1280011529__

I have created this addon for my own car (new Fiat Icon 500e) and its the only one i can test it with. 
Work in progress so expect some bugs. 😅

Example dashboard using sensors and entities provided by this addon:

![image](https://user-images.githubusercontent.com/30373916/190108698-6df2a4de-776d-45e2-8f27-1c5521f79476.png)

## Prerequisites

- Official Home Assistant MQTT Addon (recommended) running (or external broker). Broker connected to Home Assistant MQTT integration.

![image](https://user-images.githubusercontent.com/30373916/196045271-44287d3f-93ba-49c0-a72f-0bc92042efbb.png)

## Features

- Imports values like battery level, tyre pressure, odometet etc. into Home Assistant.
- Supports multiple cars on the same account. 
- Location tracking.
- Uses the same data source as the official app.
- Remote commands (open doors, switch air conditioner on, ...) are supported since version 2.0. Some commands may not work with all cars. Available commands are:
  - "UpdateLocation" (updates gps location from the car) 
  - "DeepRefresh" (gets battery charge % level)
  - "Blink" (blink lights)
  - "ChargeNOW" (starts charging)
  - "Trunk" (open/close trunk lock)
  - "HVAC" (turn on the temperature preconditioning in the car. __the official app does not support turning preconditioning off 😅 i found an hidden command for this but i don't know if it will work or have negative side effects. enable it by setting the "EnableDangerousCommands" option.__)
- Convert km to miles option.

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

You dont have to configure MQTT. At startup the Addon will automatically connect to your Home Assistant MQTT Broker.

- PIN is only needed if you want to send commands to your car. Its the same PIN used by the official app or website.
- Use DEBUG carefully. It will dump many informations to the log including session tokens and sensitive informations.
- Automatic refresh of location and battery level may drain your battery a bit more. The car have to wakeup some parts, read new values and sent them back. This will get executed every "Refresh interval" and at every command even if your car is not at home. __Recommendation:__  Better use a Home Assistant automation instead. I have setup an automation that is triggered by plugging in the charger cable and then updates the battery level (by calling DeepRefresh) every 15 minutes until its 100% or charger is disconnected. ( see here for screenshots of my automations https://github.com/wubbl0rz/FiatChamp/issues/4#issuecomment-1271866433 )
- Only set "Dangerous commands" if you want to use unoffical commands that are not present in the the official app.
- Mqtt override can be used if you want to utilize an external mqtt broker. __You do not need this if you are using the official home assistant mqtt addon.__

<img src="https://user-images.githubusercontent.com/30373916/196044104-a3f594d4-45d1-4436-af98-ca7dc88cec29.png" width="700px">

## FAQ

### Where is the data ?

inside the mqtt integration (click on "devices"). after a successful run there should be a new entry named "car" or the nickname you gave the car on the website.

![image](https://user-images.githubusercontent.com/30373916/196047443-add9ad0b-4f8f-429b-9a71-7ec0f5eae96e.png)

if not then check the error logs output of the addon.

### Why is location not working.

it should work. have a look at the attributes. only the main status says "unknown". haven't figured out yet how to fix that.

<img src="https://user-images.githubusercontent.com/30373916/196045834-0d57657a-3ef0-4361-9340-7946778158e7.png" width="300px">
