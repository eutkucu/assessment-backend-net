#!/bin/bash

# Kafka container'ına bağlan ve topic'leri sil
docker exec -it assesmentsetur-kafka-1 kafka-topics.sh --delete --topic contact-events --bootstrap-server localhost:9092
docker exec -it assesmentsetur-kafka-1 kafka-topics.sh --delete --topic report-requests --bootstrap-server localhost:9092

# Topic'leri listele
docker exec -it assesmentsetur-kafka-1 kafka-topics.sh --list --bootstrap-server localhost:9092 