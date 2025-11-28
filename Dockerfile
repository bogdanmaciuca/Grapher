FROM mcr.microsoft.com/dotnet/sdk:9.0

COPY entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

RUN echo 'export PATH="$PATH:/root/.dotnet/tools"' >> /root/.bashrc && \
    echo 'export PATH="$PATH:/root/.dotnet/tools"' >> /root/.profile && \
    echo 'export PATH="$PATH:/root/.dotnet/tools"' >> /etc/profile.d/dotnet-tools.sh

ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
