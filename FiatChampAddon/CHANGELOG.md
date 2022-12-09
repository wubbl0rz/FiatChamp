# CHANGELOG

## 3.x.x - yyyy-mm-dd
- added option to prefix entity names with the name of the car

## 3.0.7 - 2022-12-03
- fixed commands for jeep america #27

## 3.0.6 - 2022-12-03
- fix car location if altitude null 

## 3.0.5 - 2022-12-03
- fix jeep when car model is not an int.

## 3.0.4 - 2022-12-03
- fixed dodge login #24
- use fiatchamp in standalone docker container #22

## 3.0.3 - 2022-11-20
- fixed bug with region selection

## 3.0.1 - 2022-11-19
- region support. some cars only work with the correct region set. default region is europe.

## 3.0.0 - 2022-11-02
- support for different brands. (fiat, jeep, ram, dodge)
  - fiat works
  - jeep should work out of the box
  - initial support for Ram Truck
  - dodge unknown
- instant state restore after home assistant reboot (retain mqtt state messages)
- battery charge now has % unit
- time to charge now has minutes unit
- added last update (time) sensor
- fixed many sensor icons and add device\_class
- added RefreshBatteryStatus button. its the same as DeepRefresh but with a better name.

## 2.0.16 - 2022-10-22
- make mqtt user and password optional. useful when using external brokjer without authentication.

## 2.0.15 - 2022-10-19
- fix location sensor status.
- zone (home, work etc.) support. car location sensor now shows correct zone as defined in home assistant if car is in the defined radius of the zone. 
- autodetect from home assistant config if km -> miles conversion is needed. force conversion option still available.
- if using km -> miles conversion. rounded miles to .2 digits.
- 5sec delay between adding new sensors and pushing values to home assistant.

## 2.0.14 - 2022-10-16
- override mqtt option. only use this if you want to utilize an external mqtt broker.
  you do not need this if you are using the official home assistant mqtt addon.
- official mqtt addon no longer hardcoded required to start the addon. but its still the recommended way of using this addon.
- more mqtt logging

## 2.0.13 - 2022-10-16
- km to miles conversion
- distance sensors export unit of measurement to home assistant (km or mi)
- more fitting icons for some sensors
- obfuscate mail account in log
- fix debug logging

## 2.0.7 - 2022-10-10
- supports remote door locking and unlocking. dangerous command... disabled by default.
- more logging

## 2.0.6 - 2022-10-06
- fixed high memory usage with prebuilt image. as a bonus installation an upgrades should be a lot faster now.

## 2.0.0 - 2022-09-27
- added command support. you can now sent commands like "activate air conditioner" to your car.
- refresh interval now functions correctly
- auto refresh battery status option
- auto refresh location status option
- retry failed requests 3 times
- session keepalive for faster commands and less requests
- thread safe login handling
- safer option validation
- more error logging

## 1.0.56 - 2022-09-16
- added repo url to addon info page

## 1.0.55 - 2022-09-16
- better and faster retry logic if api is not reachable. 1, 2, 5 sec retry interval.
- cleaned dependencies

