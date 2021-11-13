using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace MyConsole.Producer
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
                        NumPartitions = 3
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

            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            using var producer = new ProducerBuilder<Null, string>(config).Build();

            var counter = 0;
            while (true)
            {
                counter++;
                var deliveryResult = await producer.ProduceAsync(topicName, new Message<Null, string> { Value="test" });
                Console.WriteLine($"Delivered '{deliveryResult.Value}' to '{deliveryResult.TopicPartitionOffset}', counter: {counter}");

                await Task.Delay(1000);
            }
           
        }
    }
}
