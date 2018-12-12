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
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public decimal Balance { get; set; }
     
    }
}
