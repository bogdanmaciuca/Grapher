#!/bin/sh
export PATH="/root/.dotnet/tools:$PATH"

if [ $# -eq 0 ]; then
    exec /bin/sh
else
    exec "$@"
fi

