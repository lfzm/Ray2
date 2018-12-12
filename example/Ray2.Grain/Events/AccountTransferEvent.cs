using System.Runtime.Serialization;

namespace Ray2.Grain.Events
{
    public class AccountTransferEvent : Event<long>
    {
        public AccountTransferEvent() { }
        public AccountTransferEvent(long toAccountId, decimal amount, decimal balance)
        {
            ToAccountId = toAccountId;
            Amount = amount;
            Balance = balance;
        }
        [DataMember]
        public long ToAccountId { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public decimal Balance { get; set; }
     
    }
}
