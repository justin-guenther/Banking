namespace Bank.requests;

public class WithdrawalBody
{
    public Guid customerId;
    public int amount;
    public string kind;
}