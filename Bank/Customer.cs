namespace Bank;

public class Customer
{
    public Guid CustomerId = Guid.NewGuid();
    public int saldo;
    public int debt;

    public Customer(int saldo, int debt)
    {
        this.saldo = saldo;
        this.debt = debt;
    }

    public bool hasSaldo(int amount)
    {
        return this.saldo - amount >= 0;
    }
}