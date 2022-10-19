# CHANGELOG

## 3.0.0 - 2022-11-02
- support for different brands. 
- jeep should work out of the box
- initial support for Ram Truck

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

