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

## What doesn't work (yet)?

- Eco Reports (statistics). I could not find any API yet. The official app shows it so in theory it should be accessible.
- Remote commands (open doors, switch air conditioner on, ...). Home Assistant side is already implemented and you should see the buttons but they do nothing. 

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

- PIN is currently unused. 
- Use DEBUG carefully. It will dump many informations to the log including session tokens and sensitive informations.

You dont have to configure MQTT. At startup the Addon will automatically connect to your Home Assistants MQTT Broker.

<img src="https://user-images.githubusercontent.com/30373916/190110618-2705b0e0-bcfc-4023-a572-0912b36c9d35.png" width="700px">