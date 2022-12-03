#!/usr/bin/with-contenv bashio

if [ -z ${STANDALONE+x} ]; then
  export FiatChamp_MqttUser=$(bashio::config 'OverrideMqttUser')
  export FiatChamp_MqttPw=$(bashio::config 'OverrideMqttPw')
  export FiatChamp_MqttServer=$(bashio::config 'OverrideMqttServer')
  export FiatChamp_MqttPort=$(bashio::config 'OverrideMqttPort')
  
  test "$FiatChamp_MqttUser" = "null" && export FiatChamp_MqttUser=$(bashio::services "mqtt" "username")
  test "$FiatChamp_MqttPw" = "null" && export FiatChamp_MqttPw=$(bashio::services "mqtt" "password")
  test "$FiatChamp_MqttServer" = "null" && export FiatChamp_MqttServer=$(bashio::services "mqtt" "host")
  test "$FiatChamp_MqttPort" = "null" && export FiatChamp_MqttPort=$(bashio::services "mqtt" "port")
  
  export FiatChamp_CarUnknownLocation=$(bashio::config 'CarUnknownLocation')
  export FiatChamp_Brand=$(bashio::config 'Brand')
  export FiatChamp_Region=$(bashio::config 'Region')
  export FiatChamp_StartDelaySeconds=$(bashio::config 'StartDelaySeconds')
  
  export FiatChamp_SupervisorToken=$SUPERVISOR_TOKEN
  
  export FiatChamp_FiatUser=$(bashio::config 'FiatUser')
  export FiatChamp_FiatPw=$(bashio::config 'FiatPw')
  export FiatChamp_FiatPin=$(bashio::config 'FiatPin')
  
  export FiatChamp_ConvertKmToMiles=$(bashio::config 'ConvertKmToMiles')
  
  export FiatChamp_AutoRefreshLocation=$(bashio::config 'AutoRefreshLocation')
  export FiatChamp_AutoRefreshBattery=$(bashio::config 'AutoRefreshBattery')
  export FiatChamp_EnableDangerousCommands=$(bashio::config 'EnableDangerousCommands')
  
  export FiatChamp_Debug=$(bashio::config 'Debug')
else
  echo "RUNNING IN STANDALONE MODE"
fi

cd /build/
./FiatChamp
