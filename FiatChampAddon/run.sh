#!/usr/bin/with-contenv bashio

export FiatChamp_MqttServer=$(bashio::services "mqtt" "host")
export FiatChamp_MqttUser=$(bashio::services "mqtt" "username")
export FiatChamp_MqttPw=$(bashio::services "mqtt" "password")
export FiatChamp_MqttPort=$(bashio::services "mqtt" "port")

export FiatChamp_FiatUser=$(bashio::config 'FiatUser')
export FiatChamp_FiatPw=$(bashio::config 'FiatPw')
export FiatChamp_FiatPin=$(bashio::config 'FiatPin')

DEBUG=$(bashio::config 'Debug')

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

cd /FiatClient/

if $DEBUG; then
  /root/.dotnet/dotnet run --property:WarningLevel=0
else
  /root/.dotnet/dotnet run -c Release --property:WarningLevel=0
fi
