#!/bin/bash

/usr/bin/kafka-topics --create --if-not-exists --topic contact-events --bootstrap-server kafka:29092 --partitions 1 --replication-factor 1
/usr/bin/kafka-topics --create --if-not-exists --topic report-requests --bootstrap-server kafka:29092 --partitions 1 --replication-factor 1
/usr/bin/kafka-topics --create --if-not-exists --topic report-results --bootstrap-server kafka:29092 --partitions 1 --replication-factor 1

echo "Oluşturulan topic'ler:"
/usr/bin/kafka-topics --list --bootstrap-server kafka:29092 