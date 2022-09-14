ARG BUILD_FROM
#FROM $BUILD_FROM
FROM ghcr.io/home-assistant/amd64-base:3.14

ARG BUILD_ARCH

COPY run.sh /run.sh
COPY FiatClient/ /FiatClient/

RUN apk add icu-libs
RUN apk add icu

RUN DOTNET_CLI_TELEMETRY_OPTOUT=1 curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -c 7.0

RUN chmod a+x /run.sh

CMD [ "/run.sh" ]