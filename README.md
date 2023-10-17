# ![image](https://user-images.githubusercontent.com/30373916/190129327-ca33228f-9864-418a-a65c-8be4de9592bc.png)  FiatChamp 🚗
EB Repo
Connect your FIAT, Jeep, Ram, Dodge, AlfaRomeo car 🚗 or truck 🚚 to Home Assistant. Needs a vehicle with enabled uconnect services and valid account.

- Fiat: Works ✅
- Jeep: Works ✅
- Ram: Experimental ⚠️ 
- Dodge: Unknown ⛔
- AlfaRomeo: Unknown ⛔

I have created this addon for my own car 🚗 (new Fiat Icon 500e) and its the only one i can test it with. 
Work in progress so expect some bugs 🐞. 😅

Example dashboard using sensors and entities provided by this addon:

![image](https://user-images.githubusercontent.com/30373916/190108698-6df2a4de-776d-45e2-8f27-1c5521f79476.png)

## Prerequisites 📃

- Official Home Assistant MQTT Addon (recommended) running or external mqtt broker. Broker must be connected to Home Assistant MQTT integration.

![image](https://user-images.githubusercontent.com/30373916/196045271-44287d3f-93ba-49c0-a72f-0bc92042efbb.png)

It looks like there are different uconnect services. Make sure your car works with one of the following uconnect sites. Older vehicles that only uses mopar.com do not seem to work.
- Fiat: https://myuconnect.fiat.com/
- Jeep: https://myuconnect.jeep.com
- Ram: https://connect.ramtrucks.com/
- Dodge: https://connect.dodge.com
- AlfaRomeo: https://myalfaconnect.alfaromeo.com/ 

## Features ✔️

- Imports values like battery level 🔋, tyre pressure ‍💨, odometer ⏲ etc. into Home Assistant.
- Multiple Brands: Fiat, Jeep, Ram, Dodge, AlfraRomeo
- Supports multiple cars on the same account. 🚙🚗🚕
- Location tracking.🌍
- Home Assistant zones (home 🏠, work 🏦 etc.) support.
- Uses the same data source as the official app 📱.
- Remote commands (open doors 🚪, switch air conditioner 🧊 on , ...) are supported since version 2.0. Some commands may not work with all cars. Available commands are:
  - "UpdateLocation" (updates gps location of the car) 
  - "RefreshBatteryStatus" (refresh battery level %)
  - "DeepRefresh" (same as "RefreshBatteryStatus")
  - "Blink" (blink lights)
  - "ChargeNOW" (starts charging)
  - "Trunk" (open/close trunk lock)
  - "DoorLock" (open/close doors. __See: "EnableDangerousCommands" option.__)
  - "HVAC" (turn on the temperature preconditioning in the car. __the official app does not support turning preconditioning off 😅  See: "EnableDangerousCommands" option.__)
- Convert km to miles option.

## What doesn't work (yet)? ❌

- Eco Reports (statistics). I could not find any API yet. The official app shows it so in theory it should be accessible.

## What will NEVER work? ❌

- Things the fiat api does not support. Like real time tracking or adjusting the music volume. Maybe they add some new features in the future. 

## How to install 🛠️

### Home Assistant OS or Supervised

Follow the official docs:

https://www.home-assistant.io/addons/ 

Short version:

- Switch on "Advanced Mode" in your users profile. (if you haven't already)
- Go to Settings -> Add-ons -> ADD-ON STORE
- Top right three dots. Add repo. https://github.com/wubbl0rz/FiatChamp 
- Top right three dots. Check for updates.
- Refresh Page. (F5)
- Store should show this repo now and you can install the addon.

### Standalone ( NOT RECOMMENDED ⚠️ )

When using Home Assistant as self managed docker container (like in this issue https://github.com/wubbl0rz/FiatChamp/issues/22) you can use FiatChamp in standalone mode. You need to update the container yourself and export all the needed environment variables. __This is for advanced users only.__
The supervisor token can be generated on the the user profile page inside home assistant (Long-Lived Access Tokens).

docker compose example:

``` yaml
version: "3.9"                                                                                                                                     
services:
  FiatChamp:
    image: ghcr.io/wubbl0rz/image-amd64-fiat-champ:3.0.4
    environment:
      - 'STANDALONE=True'
      - 'FiatChamp_FiatUser=user@example.com'
      - 'FiatChamp_FiatPw=123456'
      - 'FiatChamp_FiatPin=9999'
      - 'FiatChamp_SupervisorToken=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiI5NGFmMGJhZTFjYTQ0ODk2YWEzYjgzMGI5YmE4NGQxNiIsImlhdCI6MTY3MDA3Mjc
      - 'FiatChamp_StartDelaySeconds=1'
      - 'FiatChamp_Region=Europe'
      - 'FiatChamp_Brand=Fiat'
      - 'FiatChamp_CarUnknownLocation=Unknown'
      - 'FiatChamp_ConvertKmToMiles=False'
      - 'FiatChamp_MqttUser=mqtt'
      - 'FiatChamp_MqttPw=123456'
      - 'FiatChamp_MqttServer=192.168.2.132'
      - 'FiatChamp_MqttPort=1883'
```

## Options / Usage

You dont have to configure MQTT. At startup the Addon will automatically connect to your Home Assistant MQTT Broker.

- PIN is only needed if you want to send commands to your car. Its the same PIN used by the official app or website.
- Use DEBUG carefully. It will dump many informations to the log including session tokens and sensitive informations.
- Automatic refresh of location and battery level may drain your battery a bit more. The car have to wakeup some parts, read new values and sent them back. This will get executed every "Refresh interval" and at every command even if your car is not at home. __Recommendation:__  Better use a Home Assistant automation instead. I have setup an automation that is triggered by plugging in the charger cable and then updates the battery level (by calling DeepRefresh) every 15 minutes until its 100% or charger is disconnected. ( see here for screenshots of my automations https://github.com/wubbl0rz/FiatChamp/issues/4#issuecomment-1271866433 )
- Only set "Dangerous commands" if you want to use unoffical commands that are not present in the the official app.
- Mqtt override can be used if you want to utilize an external mqtt broker. __You do not need this if you are using the official home assistant mqtt addon.__

<img src="https://user-images.githubusercontent.com/30373916/199510247-dedf34f8-fa70-4788-8672-bd93da7d8325.png" width="700px">

## FAQ  🙋

### Where is the data ?

inside the mqtt integration (click on "devices"). after a successful run there should be a new entry named "car" or the nickname you gave the car on the website.

![image](https://user-images.githubusercontent.com/30373916/196047443-add9ad0b-4f8f-429b-9a71-7ec0f5eae96e.png)

if not then check the error logs output of the addon.

### Why is location not working.

it should work. have a look at the attributes. main status depends on the zones you configured in home assistant.
when the car is within the radius of a predefined zone at will show the zone name as location. otherwise status "away" or a custom string.

<img src="https://user-images.githubusercontent.com/30373916/199510683-91349d24-007d-49cd-a1f6-88f6bed0a54b.png" width="300px">

### What is DeepRefresh ? How to update my battery charging 🔋 level % ?

DeepRefresh is the "fiat language" for battery status update. The car sents only relatively rarely battery charging level % updates. 
If thats too slow for you press the "RefreshBatteryStatus" or "DeepRefresh" button (or call it in an automation) and the car should immediately update and sent back its current battery charging level %.

![image](https://user-images.githubusercontent.com/30373916/196050176-8e9405ee-0caf-4fcc-a22b-ee5acc74e86f.png)

