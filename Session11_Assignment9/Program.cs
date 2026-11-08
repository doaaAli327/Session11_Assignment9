using System;
using System.Collections.Generic;

namespace Session11_Assignment9
{
    #region Section 01: Book Classes & Library Engine

    public class Book
    {
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string[] Authors { get; set; }
        public DateTime PublicationDate { get; set; }
        public decimal Price { get; set; }

        public Book(string _ISBN, string _Title, string[] _Authors, DateTime _PublicationDate, decimal _Price)
        {
            ISBN = _ISBN;
            Title = _Title;
            Authors = _Authors;
            PublicationDate = _PublicationDate;
            Price = _Price;
        }

        public override string ToString()
        {
            string authorsStr = Authors != null ? string.Join(", ", Authors) : "Unknown";
            return $"[Book] ISBN: {ISBN}, Title: {Title}, Authors: {authorsStr}, Published: {PublicationDate.ToShortDateString()}, Price: {Price:C}";
        }
    }

    public class BookFunctions
    {
        public static string GetTitle(Book B)
        {
            return B?.Title ?? "No Title";
        }

        public static string GetAuthors(Book B)
        {
            if (B?.Authors == null) return "No Authors";
            return string.Join(", ", B.Authors);
        }

        public static string GetPrice(Book B)
        {
            return B?.Price.ToString("C") ?? "$0.00";
        }
    }

    // 1. User-defined delegate with the same signature as BookFunctions methods (takes Book, returns string)
    public delegate string BookFunctionDelegate(Book B);

    public class LibraryEngine
    {
        // Parameterized to accept the user-defined delegate
        public static void ProcessBooks(List<Book> bList, BookFunctionDelegate fPtr)
        {
            foreach (Book B in bList)
            {
                Console.WriteLine(fPtr(B));
            }
        }

        // Overload to accept built-in Func<Book, string>
        public static void ProcessBooks(List<Book> bList, Func<Book, string> fPtr)
        {
            foreach (Book B in bList)
            {
                Console.WriteLine(fPtr(B));
            }
        }
    }

    #endregion

    #region Section 02: Order Processing System & Questions Answers

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    // Part 1 — User-Defined Delegate
    public delegate decimal PriceCalculator(Order order);

    public class OrderService
    {
        // Part 5 — Events
        public event Action<Order> OrderProcessed;

        public decimal CalculateOrderPrice(Order order, PriceCalculator calculator)
        {
            return calculator(order);
        }

        public decimal CalculateOrderPrice(Order order, Func<Order, decimal> calculator)
        {
            return calculator(order);
        }

        public bool ValidateOrder(Order order, Predicate<Order> validationRule)
        {
            return validationRule(order);
        }

        public void ProcessOrder(Order order, Action<Order> action)
        {
            // Execute supplied action after processing
            action(order);
        }

        public void ProcessOrder(Order order)
        {
            // Core processing logic and raising the event
            Console.WriteLine($"[OrderService] Order #{order.Id} for {order.CustomerName} is being processed...");

            // Raise the event
            OrderProcessed?.Invoke(order);
        }
    }

    #endregion

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SECTION 01: Library Engine & Book Processing ===");
            List<Book> books = new List<Book>
            {
                new Book("123-456", "C# Programming", new String[] { "John Doe" }, new DateTime(2022, 5, 10), 150.0m),
                new Book("789-012", "Advanced ASP.NET", new String[] { "Jane Smith", "Bob Ali" }, new DateTime(2024, 1, 15), 250.0m)
            };

            Console.WriteLine("\n--- 1. User-Defined Delegate (GetTitle) ---");
            BookFunctionDelegate titleDelegate = BookFunctions.GetTitle;
            LibraryEngine.ProcessBooks(books, titleDelegate);

            Console.WriteLine("\n--- 2. Built-in Delegate Func<> (GetAuthors) ---");
            Func<Book, string> authorFunc = BookFunctions.GetAuthors;
            LibraryEngine.ProcessBooks(books, authorFunc);

            Console.WriteLine("\n--- 3. Anonymous Method (GetISBN) ---");
            BookFunctionDelegate isbnAnonymous = delegate (Book b) {
                return b?.ISBN;
            };
            LibraryEngine.ProcessBooks(books, isbnAnonymous);

            Console.WriteLine("\n--- 4. Lambda Expression (GetPublicationDate) ---");
            Func<Book, string> dateLambda = b => b?.PublicationDate.ToShortDateString();
            LibraryEngine.ProcessBooks(books, dateLambda);


            Console.WriteLine("\n==================================================");
            Console.WriteLine("=== SECTION 02: Order Processing System ===");

            Order order = new Order { Id = 101, CustomerName = "Mona Ahmed", Price = 100.0m, Quantity = 3 };
            OrderService orderService = new OrderService();

            // Part 1 & Bonus: Pricing strategies using user-defined delegate & Func
            PriceCalculator totalCalc = (o) => o.Price * o.Quantity;
            decimal standardTotal = orderService.CalculateOrderPrice(order, totalCalc);
            Console.WriteLine($"Standard Total Price: {standardTotal:C}");

            // Part 2: Func<> with Lambda for discounts
            Func<Order, decimal> vipDiscountCalc = (o) => (o.Price * o.Quantity) * 0.80m; // 20% discount
            decimal vipTotal = orderService.CalculateOrderPrice(order, vipDiscountCalc);
            Console.WriteLine($"VIP Discount Total Price (20% off): {vipTotal:C}");

            // Part 3: Predicate<> Validations
            bool isValidQuantity = orderService.ValidateOrder(order, o => o.Quantity > 0);
            bool isValidPrice = orderService.ValidateOrder(order, o => o.Price > 0);
            bool hasCustomer = orderService.ValidateOrder(order, o => !string.IsNullOrEmpty(o.CustomerName));
            Console.WriteLine($"Order Validations -> Quantity > 0: {isValidQuantity}, Price > 0: {isValidPrice}, Has Name: {hasCustomer}");

            // Part 4: Action<> Behaviors
            Action<Order> printOrder = o => Console.WriteLine($"-> Action: Order {o.Id} details printed.");
            Action<Order> sendSms = o => Console.WriteLine($"-> Action: SMS confirmation sent to {o.CustomerName}.");
            orderService.ProcessOrder(order, printOrder);
            orderService.ProcessOrder(order, sendSms);

            // Part 5 & 6: Events & Subscribing/Unsubscribing Handlers
            Console.WriteLine("\n--- Events Demonstration ---");
            Action<Order> handler1 = o => Console.WriteLine($"[Handler 1] Notification: Order #{o.Id} completed successfully.");
            Action<Order> handler2 = o => Console.WriteLine($"[Handler 2] Audit Log: Order #{o.Id} recorded in database.");

            // Subscribe handlers
            orderService.OrderProcessed += handler1;
            orderService.OrderProcessed += handler2;

            Console.WriteLine("Executing ProcessOrder with both handlers subscribed:");
            orderService.ProcessOrder(order);

            // Unsubscribe Handler 1
            orderService.OrderProcessed -= handler1;
            Console.WriteLine("\nExecuting ProcessOrder after unsubscribing Handler 1:");
            orderService.ProcessOrder(order);

            Console.WriteLine("\nAll assignments completed successfully!");
        }
    }
}

/*
    ========================================================================
    ANSWERS TO ASSIGNMENT QUESTIONS (Q1 - Q8)
    ========================================================================

    Q1: What is the difference between PriceCalculator and Func<Order, decimal>?
    - PriceCalculator is a custom, user-defined delegate explicitly declared with a specific name and signature. 
    - Func<Order, decimal> is a generic built-in delegate type provided by the .NET framework that achieves the exact same signature (takes Order, returns decimal) without requiring a custom declaration.

    Q2: What is the difference between Action<Order> and Func<Order, decimal>?
    - Action<T> represents a method that takes parameters and returns void (no return value, performs an action).
    - Func<T, TResult> represents a method that takes parameters and MUST return a value of type TResult (used for computations/transformations).

    Q3: Why does Predicate<T> return bool? What kind of problem is it designed to represent?
    - Predicate<T> returns bool because it is specifically designed to represent criteria, filtering rules, or conditions (testing whether an object satisfies a specific condition, returning true or false).

    Q4: What is the difference between a delegate and an event?
    - A delegate is a type that references methods and can be invoked or reassigned directly from anywhere it is accessible.
    - An event is a wrapper around a delegate that enforces encapsulation, allowing external classes to only add (+=) or remove (-=) handlers, preventing them from clearing or directly invoking/triggering the event from outside the class.

    Q5: Why can't external code normally invoke an event declared in another class?
    - To protect the encapsulation and integrity of the publisher class, preventing external code from accidentally or maliciously triggering, resetting, or overriding the subscriber list. Only the containing class can invoke the event.

    Q6: What happens when multiple handlers subscribe to the same event?
    - It becomes a multicast event/delegate. When the event is invoked, all subscribed handler methods are executed sequentially in the order they were added.

    Q7: Explain what this code means: orderService.OrderProcessed += HandleOrderProcessed;
    - OrderProcessed: The event being subscribed to.
    - +=: The assignment operator used to add (register) a method to the event's invocation list (multicast).
    - HandleOrderProcessed: The method (handler) that will be invoked whenever the event is raised.

    Q8 — Challenge: Explain the difference between Action<Order> and event Action<Order>?
    - Action<Order> is a delegate field that can be invoked or reassigned directly from outside the class.
    - event Action<Order> restricts external access so that external classes can only subscribe or unsubscribe using += and -=, ensuring that event firing and safety are controlled strictly inside the publisher class.
*/
