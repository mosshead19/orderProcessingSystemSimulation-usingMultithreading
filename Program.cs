using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace orderProcessingFinal
{
    internal class Program
    {
        // Shared thread-safe queues for three stages
        static BlockingCollection<Order> orderQueue = new BlockingCollection<Order>();
        static BlockingCollection<Order> processingQueue = new BlockingCollection<Order>();
        static BlockingCollection<Order> shippingQueue = new BlockingCollection<Order>();

        static void Main(string[] args)
        {
            // Generate orders dynamically
            Console.Write("Enter the number of orders to simulate: ");
            int num = int.Parse(Console.ReadLine());
            var orders = GenerateOrders(1, num);
            Console.WriteLine($"Generated {orders.Count} orders dynamically.");

            // Create threads for each stage
            Thread placementThread = new Thread(() => OrderPlacement(orders)); // uses lambda parameterized threadstart
            Thread processingThread = new Thread(OrderProcessing); //uses threadstart
            Thread shippingThread = new Thread(OrderShipping);

            // Start threads
            placementThread.Start();
            processingThread.Start();
            shippingThread.Start();

            // Wait for threads to complete
            placementThread.Join();
            processingThread.Join();
            shippingThread.Join();

            Console.WriteLine("Order processing pipeline completed!");
            Console.ReadKey();
        }

        // Order Placement Stage
        static void OrderPlacement(List<Order> orders)
        {
            foreach (var order in orders)
            {
                Console.WriteLine($"Order {order.OrderId} placed by {order.CustomerName}.");
                orderQueue.Add(order); // Add to the orderQueue
                Thread.Sleep(1); // Simulate delay (500ms for placement)
            }
            orderQueue.CompleteAdding(); // Mark the queue as complete
        }

        // Order Processing Stage
        static void OrderProcessing()
        {
            foreach (var order in orderQueue.GetConsumingEnumerable())
            {
                Console.WriteLine($"Order {order.OrderId} processing...");
                Thread.Sleep(1); // Simulate delay for processing
                Console.WriteLine($"Order {order.OrderId} processed - Items packed: [{string.Join(", ", order.Items)}].");
                processingQueue.Add(order); // Add to the processingQueue
            }
            processingQueue.CompleteAdding(); // Mark the queue as complete
        }

        // Order Shipping Stage
        static void OrderShipping()
        {
            foreach (var order in processingQueue.GetConsumingEnumerable())
            {
                Console.WriteLine($"Order {order.OrderId} shipping...");
                Thread.Sleep(1); // Simulate delay for shipping
                Console.WriteLine($"Order {order.OrderId} shipped to {order.Address}.");
                shippingQueue.Add(order); // Add to the shippingQueue
            }
        }

        // Method to generate orders dynamically
        static List<Order> GenerateOrders(int startId, int endIdRange)
        {
            var orders = new List<Order>();
            var random = new Random();
            var availableItems = new List<string> { "Laptop", "Mouse", "Keyboard", "Monitor", "Webcam", "Headset", "Printer", "USB Cable", "External Hard Drive", "Docking Station" };

            for (int i = startId; i <= endIdRange; i++)
            {
                int numberOfItems = random.Next(1, 6);
                var items = new List<string>();
                for (int j = 0; j < numberOfItems; j++)
                {
                    var randomItem = availableItems[random.Next(availableItems.Count)];
                    items.Add(randomItem);
                }

                orders.Add(new Order
                {
                    OrderId = i,
                    CustomerName = $"Customer {i}",
                    Items = items,
                    Address = $"{random.Next(1, 500)} Main St"
                });
            }
            return orders;
        }

        // Order class definition
        public class Order
        {
            public int OrderId { get; set; }
            public string CustomerName { get; set; }
            public List<string> Items { get; set; }
            public string Address { get; set; }
        }
    }
}
