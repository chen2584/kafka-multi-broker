using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace MyConsole.Consumer
{
    class Program
    {
        static async Task CreateTopicAsync(string bootstrapServers, string topicName)
        {
            var config = new AdminClientConfig { BootstrapServers = bootstrapServers };

            var adminClient = new AdminClientBuilder(config).Build();

            try
            {
                await adminClient.CreateTopicsAsync(new TopicSpecification[]
                {
                    new TopicSpecification
                    {
                        Name = topicName,
                        ReplicationFactor = 3,
                        NumPartitions = 3,
                    }
                });
            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
            }
        }

        static async Task Main(string[] args)
        {
            var bootstrapServers = "localhost:19092,localhost:29092,localhost:39092";
            var topicName = "hello-world-4";

            await CreateTopicAsync(bootstrapServers, topicName);

            var config = new ConsumerConfig()
            {
                GroupId = "hello-consumer-group",
                BootstrapServers = bootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Null, string>(config).Build();
            consumer.Subscribe(topicName);

            Console.WriteLine("Listening...");
            try
            {
                while (true)
                {
                    var consumeResult = consumer.Consume();
                    Console.WriteLine($"Message: {consumeResult.Message.Value} received from {consumeResult.TopicPartitionOffset}");
                    consumer.Commit();
                }
            }
            catch (Exception)
            {
                consumer.Close();
            }
        }
    }
}
