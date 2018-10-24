using System.Runtime.Serialization;

namespace Ray2.EventProcess
{
    /// <summary>
    /// Status of event processing, record processing version number time, 
    /// etc.
    /// </summary>
    /// <typeparam name="TStateKey"></typeparam>
    [DataContract]
    public class EventProcessState<TStateKey> : State<TStateKey>
    {
        protected override void PlayEvent(IEvent @event)
        {
            return;
        }
    }
}
