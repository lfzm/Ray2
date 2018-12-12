using System.Runtime.Serialization;

namespace Ray2.Grain.Events
{
    public class AccountAddBalanceEvent : Event<long>
    {
        public AccountAddBalanceEvent() { }
        public AccountAddBalanceEvent(decimal amount)
        {
            Amount = amount;
        }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
     
    }
}
