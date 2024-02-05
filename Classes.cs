using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Classes;

public class Currency
{
    public string Name { get; set; }
    private readonly double _exchangePriceEuroToDollar = 1.09; //Euro to Dollar
    private readonly double _exchangePriceDollarToEuro = 0.92; //Dollar to Euro

    public Currency(string name)
    {
        Name = name;
    }
    
    public double GetPrice(string currency)
    {
        if (currency == "Dollar")
        {
            return _exchangePriceEuroToDollar;
        }
        else if(currency == "Euro")
        {
            return _exchangePriceDollarToEuro;
        }
        else
        {
            Console.WriteLine("This currency is not registered!");
            return 0;
        }
    }
}

public class Dollar : Currency
{
    public Dollar() : base("Dollar"){}
}

public class Euro : Currency
{
    public Euro() : base("Euro"){}
}

public class Wallet<T> where T : Currency
{
    [Key]
    public int Id { get; set; }
    public T _currency { get; set; }
    public double _amount { get; set; }

    public Wallet(T currency, double amount)
    {
        _currency = currency;
        _amount = 0;
    }
}

public interface IWalletRepository
{
    public int CreateWallet<T>(string currency, double amount) where T : Currency; //create
    
    public double GetBalance(int id, string currency); //read

    public int AddFunds(int id, double amount, string currency); //update

    public int RemoveFunds(int id, double amount); //update

    public int ExchangeFunds(int id, string currency); //update

    public int DeleteWallet(int id); //delete
}

public class WalletRepository<U> : IWalletRepository where U : Currency
{
    private readonly DatabaseContext<U> _context;
    
    public WalletRepository(DatabaseContext<U> ctx)
    {
        _context = ctx;
    }

    public int CreateWallet<T>(string currency, double amount) where T : Currency //create
    {
        if(currency == "Dollar")
        {
            Dollar dollar = new Dollar();
            Wallet<Currency> temp = new Wallet<Currency>(dollar, amount);
            _context.Add(temp);
            _context.SaveChanges();
            return temp.Id;
        }
        else if(currency == "Euro")
        {
            Euro euro = new Euro();
            Wallet<Currency> temp = new Wallet<Currency>(euro, amount);
            _context.Add(temp);
            _context.SaveChanges();
            return temp.Id;
        }
        else
        {
            Console.WriteLine("This currency is not registered!");
            return 0;
        }
    }

    public double GetBalance(int id, string currency) //read
    {
        var wallet = _context.Wallets.Where(a => a.Id == id).FirstOrDefault();
        double price = wallet._currency.GetPrice(currency);
        string name = wallet._currency.Name;
        if (name == currency)
        {
            Console.WriteLine($"Balance of {currency} Wallet in {currency}s: {wallet._amount}");
            return wallet._amount;
        }
        else if(price != 0)
        {
            double temp = wallet._amount * price;
            Console.WriteLine($"Balance of {name} Wallet in {currency}s: {temp}");
            return temp;
        }
        else
        {
            Console.WriteLine("This currency is not registered!");
            return 0;
        }
    }
    
    public int AddFunds(int id, double amount, string currency) //update
    {
        var wallet = _context.Wallets.Where(a => a.Id == id).FirstOrDefault();
        double price = wallet._currency.GetPrice(currency);
        string name = wallet._currency.Name;
        if (name == currency)
        {
            wallet._amount += amount;
            _context.SaveChanges();
            Console.WriteLine($"Added {amount} {currency}s to the {name} Wallet!");
            return 1; //success
        }
        else if(price != 0)
        {
            wallet._amount += amount * price;
            _context.SaveChanges();
            Console.WriteLine($"Added {amount} {currency}s to the {name} Wallet!");
            return 1;
        }
        else
        {
            Console.WriteLine("This currency is not registered!");
            return 0; //error
        }
    }

    public int RemoveFunds(int id, double amount) //update
    {
        var wallet = _context.Wallets.Where(a => a.Id == id).FirstOrDefault();
        string name = wallet._currency.Name;
        wallet._amount -= amount;
        _context.SaveChanges();
        Console.WriteLine($"Removed {amount} {name}s from the {name} Wallet!");
        return 1;
    }

    public int ExchangeFunds(int id, string currency) //update
    {
        var wallet = _context.Wallets.Where(a => a.Id == id).FirstOrDefault();
        double price = wallet._currency.GetPrice(currency);
        string name = wallet._currency.Name;
        if (name == currency)
        {
            Console.WriteLine("You cannot convert to the same currency!");
            return 0;
        }
        else if(price != 0)
        {
            wallet._amount *= price;
            wallet._currency.Name = currency;
            _context.SaveChanges();
            Console.WriteLine($"Converted Wallet from {name} to {currency}. Balance: {wallet._amount}");
            return 1;
        }
        else
        {
            Console.WriteLine("This currency is not registered!");
            return 0;
        }
    }

    public int DeleteWallet(int id) //delete
    {
        var wallet = _context.Wallets.Where(a => a.Id == id).FirstOrDefault();
        if (wallet == null)
        {
            return 0;
        }
        _context.Remove<Wallet<U>>(wallet);
        _context.SaveChanges();
        return 1;
    }
}

public class DatabaseContext<T> : DbContext where T : Currency
{
    public DatabaseContext(DbContextOptions<DatabaseContext<T>> options) : base(options){}
    
    public DbSet<Wallet<T>> Wallets { get; set; }
}