#!/usr/bin/with-contenv bashio

if [ -z ${STANDALONE+x} ]; then
  export FiatUconnect_MqttUser=$(bashio::config 'OverrideMqttUser')
  export FiatUconnect_MqttPw=$(bashio::config 'OverrideMqttPw')
  export FiatUconnect_MqttServer=$(bashio::config 'OverrideMqttServer')
  export FiatUconnect_MqttPort=$(bashio::config 'OverrideMqttPort')
  
  test "$FiatUconnect_MqttUser" = "null" && export FiatUconnect_MqttUser=$(bashio::services "mqtt" "username")
  test "$FiatUconnect_MqttPw" = "null" && export FiatUconnect_MqttPw=$(bashio::services "mqtt" "password")
  test "$FiatUconnect_MqttServer" = "null" && export FiatUconnect_MqttServer=$(bashio::services "mqtt" "host")
  test "$FiatUconnect_MqttPort" = "null" && export FiatUconnect_MqttPort=$(bashio::services "mqtt" "port")
  
  export FiatUconnect_CarUnknownLocation=$(bashio::config 'CarUnknownLocation')
  export FiatUconnect_Brand=$(bashio::config 'Brand')
  export FiatUconnect_Region=$(bashio::config 'Region')
  export FiatUconnect_StartDelaySeconds=$(bashio::config 'StartDelaySeconds')
  
  export FiatUconnect_SupervisorToken=$SUPERVISOR_TOKEN
  
  export FiatUconnect_FiatUser=$(bashio::config 'FiatUser')
  export FiatUconnect_FiatPw=$(bashio::config 'FiatPw')
  export FiatUconnect_FiatPin=$(bashio::config 'FiatPin')
  export FiatUconnect_Debug=$(bashio::config 'Debug')
else
  echo "RUNNING IN STANDALONE MODE"
fi

cd /build/
./FiatUconnect
