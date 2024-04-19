FROM scratch AS test

FROM downloads.unstructured.io/unstructured-io/unstructured:0.12.2 as bundle

WORKDIR /app
EXPOSE 80

USER root
COPY web/OraculumIngestion/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
RUN dnf install epel-release && \
    dnf config-manager --set-enabled crb && \
    dnf install --nogpgcheck https://mirrors.rpmfusion.org/free/el/rpmfusion-free-release-$(rpm -E %rhel).noarch.rpm -y && \
    dnf install --nogpgcheck https://mirrors.rpmfusion.org/nonfree/el/rpmfusion-nonfree-release-$(rpm -E %rhel).noarch.rpm -y && \
    dnf install ffmpeg ffmpeg-devel -y && \
    python3.10 -m spacy download it_core_news_sm


COPY web/OraculumIngestion/ .

CMD ["python3.10", "main.py"]
