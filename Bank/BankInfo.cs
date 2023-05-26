using Boerse.abstractions;

namespace Bank;

public static class BankInfo
{
    public static int Portfolio;
    public static List<Customer> Customers;
    public static IEnumerable<Shares>? Shares;
    public static int CashReserve;
    public static int Debts;
    
    static BankInfo()
    {
        CashReserve = 35000;
        Debts = 10000;
        Portfolio = CashReserve + Debts;
        Customers = new List<Customer>();
    }

    public static Customer GetCustomerIndex(Guid customerId)
    {
        int index = Customers.FindIndex(c => c.CustomerId == customerId);
        return Customers[index];
    }
}