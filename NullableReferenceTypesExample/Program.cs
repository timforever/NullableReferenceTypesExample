using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullableReferenceTypesExample
{
    class Program
    {
        /// <summary>
        /// This simple console apps demonstrates some usages of the nullable reference types feature
        /// in C#.
        /// https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references
        /// 
        /// In this project, the feature is turned on project wide in the project files. However, it
        /// can also be turned on or off in individual source files by using compiler directives such
        /// as "#nullable enable" and "#nullable disable"
        /// 
        /// Look thru the code and comments to see how this feature works. Check out the warnings that
        /// Visual Studio shows to warn you when you're using a non-nullable reference with a null value
        /// incorrectly. The comments will tell you how to fix the warnings and explain their reasoning.
        /// </summary>
        static void Main()
        {
            Console.WriteLine("Nullable References Analysis Demo using Pizza!");
            Console.WriteLine();
            Console.WriteLine();

            // Bad: This will generate a compiler warning because we're passing a nullable reference to
            //      a non-nullable reference type:
            List<Topping>? nullToppings = null;
            _ = new Pizza(nullToppings);

            // Good: this will fix the warning by passing in a non-null reference.
            List<Topping> noToppings = new List<Topping>();
            var cheesePizza = new Pizza(noToppings);

            Console.WriteLine(cheesePizza.GetDescription("Cheese Pizza"));

            var meatLovers = new Pizza(
                new List<Topping> { Topping.Ham, Topping.Meatball, Topping.Pepperoni, Topping.Sausage, Topping.Bacon },
                new List<Cheese> { CommonCheeses.Mozzarella, CommonCheeses.Parmesan });

            Console.WriteLine(meatLovers.GetDescription("Meat Lovers' Pizza"));

            // We're allowed to pass null in as parameters in these 2 lines because these parameters explicitly
            // say that they allow null.
            var isItAllowed = new Pizza(new List<Topping> { Topping.Ham, Topping.Pineapple }, null);
            Console.WriteLine(isItAllowed.GetDescription(null));

            // Cheesy analysis:
            Console.WriteLine();
            Console.WriteLine("What is Mozzarella cheese made from?");
            
            string animal = CommonCheeses.Mozzarella.AnimalType.Trim(); // Uh-oh. Warning! What's the fix?
            // string animal = CommonCheeses.Mozzarella.AnimalType?.Trim() ?? "Unknown animal";

            // Unlike above, we don't need to check for null because the property type is non-nullable:
            _ = CommonCheeses.Mozzarella.CheeseName.Trim();
            
            Console.WriteLine($"Animal type: {animal}.");
            Console.WriteLine(CommonCheeses.Mozzarella.GetCheeseDescription());
        }
    }

    public class Pizza
    {
        public List<Topping> Toppings { get; }

        // Here we need to use the null forgiveness operator, the '!' postfix operator, because we're getting a compiler warning:
        // "Non-nullable property 'Cheeses' must contain a non-null value when exiting constructor."
        // Adding the assignment '= null!' tells the compiler that it ok that the value is null even though its declared as non-nullable
        // and the compiler will ignore the warning.
        //
        // In this example, we know that it is safe to do this because InitCheeses() is
        // called from the constructor and it will assign a non-null value to Cheeses.
        public List<Cheese> Cheeses { get; private set; } // = null!;

        // Because we're in a nullable reference context (it's project wide in this example), this constructor
        // is stating that toppings cannot be null, and if a nullable reference is passed in, it will generate
        // a warning.
        // On the other hand, 'cheeses' is stating that null may be passed in. This is indicated by the '?' annotation
        // on List<Cheese>?.
        /// <summary>
        /// Make me a pizza!
        /// </summary>
        /// <param name="toppings">Pizza toppings, such as pepperoni, sausage, etc.</param>
        /// <param name="cheeses">If null, Cheeses will default to Mozzarella.</param>
        public Pizza(List<Topping> toppings, List<Cheese>? cheeses = default)
        {
            Toppings = toppings;

            InitCheeses(cheeses);
        }

        private void InitCheeses(List<Cheese>? cheeses)
        {
            Cheeses = cheeses; // <-- This causes a warning because cheeses may be null here. Instead, check for null:
            Cheeses = cheeses ?? new List<Cheese> { CommonCheeses.Mozzarella };
        }

        public string GetToppingsText()
        {
            // Notice that we don't need to check if Toppings is null or not before using it.
            // We've already stated null is not allowed for Toppings. If the caller of Pizza has passed in null, they
            // will be warned by the compiler, and the resulting NullException will be their fault, as we have
            // explicitly stated that null is not allowed.
            // If you move your mouse over Toppings, Visual Studio should show the text:
            // "'Toppings' is not null here." This is a quick way to check if a reference may be null or not.
            if (Toppings.Count > 0)
            {
                return String.Join(", ", Toppings);
            }

            // Whoops - we're declaring that we won't return null, so the following would give us a warning:
            // return null;

            // Instead, let's return either an empty string, or more helpful text:
            return "no toppings";
        }

        public string GetDescription(string? pizzaName)
        {
            var cheeseStr = String.Join(", ", Cheeses.Select(c => c.CheeseName));

            return String.IsNullOrWhiteSpace(pizzaName)
                ? $"This pizza is made with {cheeseStr} cheese, and has {GetToppingsText()} on it."
                : $"{pizzaName} is made with {cheeseStr} cheese, and has {GetToppingsText()} on it.";
        }
    }

    public enum Topping
    {
        Pepperoni,
        Sausage,
        Basil,
        Pepper,
        Onion,
        Meatball,
        Ham,
        Pineapple,
        Olives,
        Bacon
    }

    // I'm not a fan of an object being so mutable with public setters, but this is just
    // an example.
    public class Cheese
    {
        // We're declaring that CheeseName can't be null, so we need to initialize it with
        // an empty string.
        public string CheeseName { get; set; } = "";

        public double? FatPercentage { get; set; }
        
        
        // The '?' indicates that this reference may be null.
        public string? AnimalType { get; set; }

        public string GetCheeseDescription()
        {
            var builder = new StringBuilder();

            var cheeseName = String.IsNullOrWhiteSpace(CheeseName) ? "An Unknown" : CheeseName;

            builder.Append($"{cheeseName} cheese");

            // Try using "Go To Definition" on IsNullOrWhiteSpace()... Notice that it uses the
            // attribute [NotNullWhen(false)] to indicate to the compiler that the reference has
            // been checked for null. These types of attributes can be used give more details
            // about when a reference might be null or not.
            if (!String.IsNullOrWhiteSpace(AnimalType))
            {
                builder.Append($" is made from {AnimalType} milk");
            }

            if (!String.IsNullOrWhiteSpace(AnimalType) && FatPercentage.HasValue)
            {
                builder.Append(" and");
            }

            if (FatPercentage.HasValue)
            {
                builder.Append($" has {FatPercentage:P} fat milk");
            }

            builder.Append(".");

            return builder.ToString();
        }
    }

    public static class CommonCheeses
    {
        public static Cheese Mozzarella => new Cheese
        {
            CheeseName = "Mozzarella",
            AnimalType = "Italian buffalo",
            FatPercentage = .22
        };

        public static Cheese Parmesan => new Cheese
        {
            CheeseName = "Parmesan",
            AnimalType = "cow",
            FatPercentage = .32
        };
    }
}
