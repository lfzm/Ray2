namespace Ray2.EventProcess
{
    /// <summary>
    /// Status of event processing, record processing version number time, 
    /// etc.
    /// </summary>
    /// <typeparam name="TStateKey"></typeparam>
    public class EventProcessState<TStateKey> : State<TStateKey>
    {
        protected override void PlayEvent(IEvent @event)
        {
            return;
        }
    }
}
