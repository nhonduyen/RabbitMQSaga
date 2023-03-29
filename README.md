RabbitMQSaga

.NET Core + RabbitMQ + Saga pattern to undo fail transactions 

Refferal link: https://mahedee.net/distributed-transaction-using-saga-rabbitmq-aspnetcore/

Docker install RabbitMQ
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.11-management

Visit link  http://localhost:15672/ to monitor

To start RabbitMQ 
Cd C:\Program Files\RabbitMQ Server\rabbitmq_server-3.11.9\sbin
rabbitmq-plugins enable rabbitmq_management
net stop RabbitMQ
net start RabbitMQ
