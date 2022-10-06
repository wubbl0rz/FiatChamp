# CHANGELOG

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

