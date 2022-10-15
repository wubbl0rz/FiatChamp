#!/usr/bin/with-contenv bashio

export FiatChamp_MqttServer=$(bashio::services "mqtt" "host")
export FiatChamp_MqttUser=$(bashio::services "mqtt" "username")
export FiatChamp_MqttPw=$(bashio::services "mqtt" "password")
export FiatChamp_MqttPort=$(bashio::services "mqtt" "port")

export FiatChamp_FiatUser=$(bashio::config 'FiatUser')
export FiatChamp_FiatPw=$(bashio::config 'FiatPw')
export FiatChamp_FiatPin=$(bashio::config 'FiatPin')

export FiatChamp_ConvertKmToMiles=$(bashio::config 'ConvertKmToMiles')

export FiatChamp_AutoRefreshLocation=$(bashio::config 'AutoRefreshLocation')
export FiatChamp_AutoRefreshBattery=$(bashio::config 'AutoRefreshBattery')
export FiatChamp_EnableDangerousCommands=$(bashio::config 'EnableDangerousCommands')

export FiatChamp_Debug=$(bashio::config 'Debug')

cd /build/
./FiatChamp
