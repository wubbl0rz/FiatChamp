# BUILD IMAGE
ARG BUILD_FROM
FROM $BUILD_FROM

ARG BUILD_ARCH

RUN apk add icu-libs
RUN apk add icu

COPY run.sh /run.sh
RUN chmod a+x /run.sh

COPY FiatClient/build/$BUILD_ARCH/ /build/.

LABEL org.opencontainers.image.source https://github.com/wubbl0rz/FiatChamp

CMD [ "/run.sh" ]
