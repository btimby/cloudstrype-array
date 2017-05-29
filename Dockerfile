FROM mono:4

COPY CloudstrypeArray/CloudstrypeArray/bin/Debug/ /array/

VOLUME /mnt/data/array/:/data/

ENV ARRAY_SIZE="20GB"
ENV ARRAY_NAME="154a7f48-3c11-4ea7-b323-506dc19282b1"
ENV ARRAY_SERVER="tcp://192.168.1.83:8765/"

CMD ["mono", "/array/CloudstrypeArray.exe", "--path=/data"]
